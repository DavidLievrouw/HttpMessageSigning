#if NETCORE
using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.WebEncoders.Testing;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.AspNetCore {
    public class SignedRequestAuthenticationHandlerTests {
        private readonly Microsoft.AspNetCore.Authentication.ISystemClock _clock;
        private readonly UrlEncoder _encoder;
        private readonly HttpRequest _httpRequest;
        private readonly ILoggerFactory _logger;
        private readonly SignedRequestAuthenticationOptions _options;
        private readonly IRequestSignatureVerifier _requestSignatureVerifier;
        private readonly string _schemeName;
        private readonly SignedRequestAuthenticationHandlerForTests _sut;

        public SignedRequestAuthenticationHandlerTests() {
            FakeFactory.Create(out _logger, out _clock, out _requestSignatureVerifier);
            _encoder = new UrlTestEncoder();
            _schemeName = "tests-scheme";
            _options = new SignedRequestAuthenticationOptions {Realm = "test-app"};
            var optionsMonitor = A.Fake<IOptionsMonitor<SignedRequestAuthenticationOptions>>();
            A.CallTo(() => optionsMonitor.Get(_schemeName)).Returns(_options);
            _sut = new SignedRequestAuthenticationHandlerForTests(optionsMonitor, _encoder, _clock, _requestSignatureVerifier, _logger);
            _httpRequest = new DefaultHttpContext().Request;
            _sut.InitializeAsync(
                new AuthenticationScheme(
                    _schemeName,
                    _schemeName,
                    _sut.GetType()),
                _httpRequest.HttpContext);
        }

        public class HandleAuthenticateAsync : SignedRequestAuthenticationHandlerTests {
            [Fact]
            public async Task GivenRequestWithoutAuthorizationHeader_ReturnsNoResult() {
                _httpRequest.Headers.Remove("Authorization");

                var actual = await _sut.DoAuthenticate();

                var expected = AuthenticateResult.NoResult();
                actual.Should().BeEquivalentTo(expected);
            }

            [Fact]
            public async Task GivenRequestWithInvalidAuthorizationHeader_ReturnsNoResult() {
                _httpRequest.Headers["Authorization"] = "{nonsense}";

                var actual = await _sut.DoAuthenticate();

                var expected = AuthenticateResult.NoResult();
                actual.Should().BeEquivalentTo(expected);
            }

            [Fact]
            public async Task GivenRequestWithAuthorizationHeaderForAnotherScheme_ReturnsNoResult() {
                _httpRequest.Headers["Authorization"] = "Basic abc123";

                var actual = await _sut.DoAuthenticate();

                var expected = AuthenticateResult.NoResult();
                actual.Should().BeEquivalentTo(expected);
            }

            [Fact]
            public async Task WhenVerificationFails_ReturnsFailureResult() {
                _httpRequest.Headers["Authorization"] = "tests-scheme abc123";

                var cause = SignatureVerificationFailure.InvalidSignatureString("Invalid signature");
                A.CallTo(() => _requestSignatureVerifier.VerifySignature(_httpRequest, _options))
                    .Returns(new RequestSignatureVerificationResultFailure(
                        new Client("app1", "Unit test app", new CustomSignatureAlgorithm("test"), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1)),
                        new Signature(), 
                        cause));

                var actual = await _sut.DoAuthenticate();

                actual.Succeeded.Should().BeFalse();
                actual.Failure.Should().BeAssignableTo<SignatureVerificationException>();
                actual.Failure.Message.Should().Be(cause.ToString());
            }

            [Fact]
            public async Task WhenVerificationFails_InvokesConfiguredCallback() {
                _httpRequest.Headers["Authorization"] = "tests-scheme abc123";

                var cause = SignatureVerificationFailure.InvalidSignatureString("Invalid signature");
                var failureResult = new RequestSignatureVerificationResultFailure(
                    new Client("app1", "Unit test app", new CustomSignatureAlgorithm("test"), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1)),
                    new Signature(), 
                    cause);
                A.CallTo(() => _requestSignatureVerifier.VerifySignature(_httpRequest, _options))
                    .Returns(failureResult);
                
                RequestSignatureVerificationResult resultFromCallback = null;
                _options.OnIdentityVerificationFailed = (request, failure) => {
                    resultFromCallback = failure;
                    return Task.CompletedTask;
                };
                
                await _sut.DoAuthenticate();

                resultFromCallback.Should().Be(failureResult);
            }

            [Fact]
            public async Task WhenVerificationReturnsAnUnknownResult_ReturnsFailureResult() {
                _httpRequest.Headers["Authorization"] = "tests-scheme abc123";

                A.CallTo(() => _requestSignatureVerifier.VerifySignature(_httpRequest, _options))
                    .Returns(new UnknownResult(new Client("app1", "Unit test app", new CustomSignatureAlgorithm("test"), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1)), new Signature()));

                var actual = await _sut.DoAuthenticate();

                actual.Succeeded.Should().BeFalse();
            }

            [Fact]
            public async Task WhenVerificationSucceeds_ReturnsSuccessResult() {
                _httpRequest.Headers["Authorization"] = "tests-scheme abc123";

                var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] {new Claim("name", "john.doe")}));
                A.CallTo(() => _requestSignatureVerifier.VerifySignature(_httpRequest, _options))
                    .Returns(new RequestSignatureVerificationResultSuccess(
                        new Client("app1", "Unit test app", new CustomSignatureAlgorithm("test"), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1)),
                        new Signature(), 
                        principal));

                var actual = await _sut.DoAuthenticate();

                actual.Succeeded.Should().BeTrue();
                actual.Ticket.Principal.Should().Be(principal);
                actual.Ticket.AuthenticationScheme.Should().Be("tests-scheme");
            }
            
            [Fact]
            public async Task WhenVerificationSucceeds_InvokesConfiguredCallback() {
                _httpRequest.Headers["Authorization"] = "tests-scheme abc123";

                var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] {new Claim("name", "john.doe")}));
                var successResult = new RequestSignatureVerificationResultSuccess(
                    new Client("app1", "Unit test app", new CustomSignatureAlgorithm("test"), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1)),
                    new Signature(), 
                    principal);
                A.CallTo(() => _requestSignatureVerifier.VerifySignature(_httpRequest, _options))
                    .Returns(successResult);

                RequestSignatureVerificationResult resultFromCallback = null;
                _options.OnIdentityVerified = (request, success) => {
                    resultFromCallback = success;
                    return Task.CompletedTask;
                };

                await _sut.DoAuthenticate();

                resultFromCallback.Should().Be(successResult);
            }

            private class UnknownResult : RequestSignatureVerificationResult {
                public UnknownResult(Client client, Signature signature) : base(client, signature) { }
                public override bool IsSuccess => false;
            }
        }

        public class HandleChallengeAsync : SignedRequestAuthenticationHandlerTests {
            [Fact]
            public async Task AddsWWWAuthenticateToResponse() {
                await _sut.DoChallenge();
                var actualResponse = _sut.GetResponse();
                actualResponse.Headers.Should().ContainKey("WWW-Authenticate");
                actualResponse.Headers["WWW-Authenticate"].ToString().Should().Be("tests-scheme realm=\"test-app\"");
            }
            
            [Fact]
            public async Task SetsResponseCodeTo401() {
                await _sut.DoChallenge();
                var actualResponse = _sut.GetResponse();
                actualResponse.StatusCode.Should().Be(401);
            }
            
            [Fact]
            public async Task WhenResponseHasBeenSetByOtherMiddleware_AddsWWWAuthenticateToResponse() {
                _sut.GetResponse().StatusCode = 302;
                
                await _sut.DoChallenge();
                
                var actualResponse = _sut.GetResponse();
                actualResponse.Headers.Should().ContainKey("WWW-Authenticate");
                actualResponse.Headers["WWW-Authenticate"].ToString().Should().Be("tests-scheme realm=\"test-app\"");
            }
            
            [Fact]
            public async Task WhenResponseHasBeenSetByOtherMiddleware_DoesNotChangeResponseStatusCode() {
                _sut.GetResponse().StatusCode = 302;
                
                await _sut.DoChallenge();
                
                var actualResponse = _sut.GetResponse();
                actualResponse.StatusCode.Should().Be(302);
            }
        }

        private class SignedRequestAuthenticationHandlerForTests : SignedRequestAuthenticationHandler {
            public SignedRequestAuthenticationHandlerForTests(IOptionsMonitor<SignedRequestAuthenticationOptions> options, UrlEncoder encoder,
                Microsoft.AspNetCore.Authentication.ISystemClock clock, IRequestSignatureVerifier requestSignatureVerifier, ILoggerFactory logger = null) : base(options, encoder, clock, requestSignatureVerifier, logger) { }

            internal Task DoChallenge() {
                return HandleChallengeAsync(new AuthenticationProperties());
            }

            internal Task<AuthenticateResult> DoAuthenticate() {
                return base.HandleAuthenticateAsync();
            }

            internal HttpResponse GetResponse() {
                return Response;
            }
        }
    }
}
#endif