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

namespace Dalion.HttpMessageSigning.Verification.AuthenticationScheme {
    public class SignedRequestAuthenticationHandlerTests {
        private readonly Microsoft.AspNetCore.Authentication.ISystemClock _clock;
        private readonly UrlEncoder _encoder;
        private readonly HttpRequest _httpRequest;
        private readonly ILoggerFactory _logger;
        private readonly IOptionsMonitor<SignedRequestAuthenticationOptions> _options;
        private readonly IRequestSignatureVerifier _requestSignatureVerifier;
        private readonly string _schemeName;
        private readonly SignedRequestAuthenticationHandlerForTests _sut;

        public SignedRequestAuthenticationHandlerTests() {
            FakeFactory.Create(out _options, out _logger, out _clock, out _requestSignatureVerifier);
            _encoder = new UrlTestEncoder();
            _schemeName = "tests-scheme";
            A.CallTo(() => _options.Get(_schemeName)).Returns(new SignedRequestAuthenticationOptions {Realm = "test-app"});
            _sut = new SignedRequestAuthenticationHandlerForTests(_options, _logger, _encoder, _clock, _requestSignatureVerifier);
            _httpRequest = new DefaultHttpContext().Request;
            _sut.InitializeAsync(
                new Microsoft.AspNetCore.Authentication.AuthenticationScheme(
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

                var cause = new SignatureVerificationException("Invalid signature");
                A.CallTo(() => _requestSignatureVerifier.VerifySignature(_httpRequest))
                    .Returns(new RequestSignatureVerificationResultFailure(
                        new Client("app1", new CustomSignatureAlgorithm("test")),
                        new Signature(), 
                        cause));

                var actual = await _sut.DoAuthenticate();

                actual.Succeeded.Should().BeFalse();
                actual.Failure.Should().Be(cause);
            }

            [Fact]
            public async Task WhenVerificationReturnsAnUnknownResult_ReturnsFailureResult() {
                _httpRequest.Headers["Authorization"] = "tests-scheme abc123";

                A.CallTo(() => _requestSignatureVerifier.VerifySignature(_httpRequest))
                    .Returns(new UnknownResult(new Client("app1", new CustomSignatureAlgorithm("test")), new Signature()));

                var actual = await _sut.DoAuthenticate();

                actual.Succeeded.Should().BeFalse();
            }

            [Fact]
            public async Task WhenVerificationSucceeds_ReturnsSuccessResult() {
                _httpRequest.Headers["Authorization"] = "tests-scheme abc123";

                var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] {new Claim("name", "john.doe")}));
                A.CallTo(() => _requestSignatureVerifier.VerifySignature(_httpRequest))
                    .Returns(new RequestSignatureVerificationResultSuccess(
                        new Client("app1", new CustomSignatureAlgorithm("test")),
                        new Signature(), 
                        principal));

                var actual = await _sut.DoAuthenticate();

                actual.Succeeded.Should().BeTrue();
                actual.Ticket.Principal.Should().Be(principal);
                actual.Ticket.AuthenticationScheme.Should().Be("tests-scheme");
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
        }

        private class SignedRequestAuthenticationHandlerForTests : SignedRequestAuthenticationHandler {
            public SignedRequestAuthenticationHandlerForTests(IOptionsMonitor<SignedRequestAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder,
                Microsoft.AspNetCore.Authentication.ISystemClock clock, IRequestSignatureVerifier requestSignatureVerifier) : base(options, logger, encoder, clock,
                requestSignatureVerifier) { }

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