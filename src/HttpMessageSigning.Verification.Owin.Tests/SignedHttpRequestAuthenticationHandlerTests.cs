using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.Owin {
    public class SignedHttpRequestAuthenticationHandlerTests {
        private readonly SignedHttpRequestAuthenticationOptions _options;
        private readonly OwinRequest _request;
        private readonly OwinResponse _response;
        private readonly SignedHttpRequestAuthenticationHandler _sut;

        public SignedHttpRequestAuthenticationHandlerTests() {
            _sut = new SignedHttpRequestAuthenticationHandler();

            _options = new SignedHttpRequestAuthenticationOptions {
                Realm = "UnitTests",
                Scheme = "TestScheme",
                RequestSignatureVerifier = A.Fake<IRequestSignatureVerifier>()
            };

            _request = new OwinRequest {
                Host = new HostString("unittest.com:9000")
            };
            _response = new OwinResponse();
            var owinContext = A.Fake<IOwinContext>();
            A.CallTo(() => owinContext.Request).Returns(_request);
            A.CallTo(() => owinContext.Response).Returns(_response);

            var init = _sut.GetType().GetMethod("Initialize", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            init.Invoke(_sut, new object[] {_options, owinContext});
        }

        public class AuthenticateCoreAsync : SignedHttpRequestAuthenticationHandlerTests {
            private readonly Func<Task<AuthenticationTicket>> _method;

            public AuthenticateCoreAsync() {
                var method = _sut.GetType().GetMethod(nameof(AuthenticateCoreAsync), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                _method = () => (Task<AuthenticationTicket>) method.Invoke(_sut, Array.Empty<object>());
            }

            [Fact]
            public async Task WhenRequestDoesNotContainAuthorizationHeader_ReturnsNull() {
                _request.Headers.Remove("Authorization");

                var actual = await _method();

                actual.Should().BeNull();
            }

            [Fact]
            public async Task WhenRequestDoesNotContainValidAuthorizationHeader_ReturnsNull() {
                _request.Headers["Authorization"] = "{nonsense}";

                var actual = await _method();

                actual.Should().BeNull();
            }

            [Fact]
            public async Task WhenRequestDoesNotContainAuthorizationHeaderOfExpectedScheme_ReturnsNull() {
                _request.Headers["Authorization"] = "Basic abc123";

                var actual = await _method();

                actual.Should().BeNull();
            }

            [Fact]
            public async Task WhenSignatureVerificationSucceeds_ReturnsAuthenticationTicket() {
                _request.Headers["Authorization"] = "TestScheme abc123";

                var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] {new Claim("name", "john.doe")}));
                var successResult = new RequestSignatureVerificationResultSuccess(
                    new Client("c1", "test", SignatureAlgorithm.CreateForVerification("s3cr3t"), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1)),
                    new HttpRequestForVerification(),
                    principal);
                A.CallTo(() => _options.RequestSignatureVerifier.VerifySignature(
                        A<IOwinRequest>.That.Matches(ConvertedRequest),
                        A<SignedHttpRequestAuthenticationOptions>._))
                    .Returns(successResult);

                var actual = await _method();

                actual.Should().BeEquivalentTo(new AuthenticationTicket(principal.Identity as ClaimsIdentity, new AuthenticationProperties()));
            }

            [Fact]
            public async Task WhenSignatureVerificationSucceeds_InvokesConfiguredCallback() {
                _request.Headers["Authorization"] = "TestScheme abc123";

                var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] {new Claim("name", "john.doe")}));
                var successResult = new RequestSignatureVerificationResultSuccess(
                    new Client("c1", "test", SignatureAlgorithm.CreateForVerification("s3cr3t"), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1)),
                    new HttpRequestForVerification(),
                    principal);
                A.CallTo(() => _options.RequestSignatureVerifier.VerifySignature(
                        A<IOwinRequest>.That.Matches(ConvertedRequest),
                        A<SignedHttpRequestAuthenticationOptions>._))
                    .Returns(successResult);

                RequestSignatureVerificationResult resultFromCallback = null;
                _options.OnIdentityVerified = (request, success) => {
                    resultFromCallback = success;
                    return Task.CompletedTask;
                };
                
                await _method();

                resultFromCallback.Should().Be(successResult);
            }
            
            [Fact]
            public async Task WhenSignatureVerificationFails_ReturnsNull() {
                _request.Headers["Authorization"] = "TestScheme abc123";

                var failureResult = new RequestSignatureVerificationResultFailure(
                    new Client("c1", "test", SignatureAlgorithm.CreateForVerification("s3cr3t"), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1)),
                    new HttpRequestForVerification(),
                    SignatureVerificationFailure.HeaderMissing("A header is missing.", null));
                A.CallTo(() => _options.RequestSignatureVerifier.VerifySignature(
                        A<IOwinRequest>.That.Matches(ConvertedRequest),
                        A<SignedHttpRequestAuthenticationOptions>._))
                    .Returns(failureResult);

                var actual = await _method();

                actual.Should().BeNull();
            }
            
            [Fact]
            public async Task WhenSignatureVerificationFails_InvokesConfiguredCallback() {
                _request.Headers["Authorization"] = "TestScheme abc123";

                var failureResult = new RequestSignatureVerificationResultFailure(
                    new Client("c1", "test", SignatureAlgorithm.CreateForVerification("s3cr3t"), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1)),
                    new HttpRequestForVerification(),
                    SignatureVerificationFailure.HeaderMissing("A header is missing.", null));
                A.CallTo(() => _options.RequestSignatureVerifier.VerifySignature(
                        A<IOwinRequest>.That.Matches(ConvertedRequest),
                        A<SignedHttpRequestAuthenticationOptions>._))
                    .Returns(failureResult);

                RequestSignatureVerificationResult resultFromCallback = null;
                _options.OnIdentityVerificationFailed = (request, failure) => {
                    resultFromCallback = failure;
                    return Task.CompletedTask;
                };
                
                await _method();

                resultFromCallback.Should().Be(failureResult);
            }

            internal Expression<Func<IOwinRequest, bool>> ConvertedRequest => r => r.Host == new HostString("unittest.com:9000");
        }

        public class ApplyResponseChallengeAsync : SignedHttpRequestAuthenticationHandlerTests {
            private readonly Func<Task> _method;

            public ApplyResponseChallengeAsync() {
                var method = _sut.GetType().GetMethod(nameof(ApplyResponseChallengeAsync), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                _method = () => (Task) method.Invoke(_sut, Array.Empty<object>());
            }

            [Fact]
            public async Task WhenResponseIsNot401_DoesNotChangeWWWAuthenticateHeader() {
                _response.StatusCode = 200;
                await _method();
                _response.Headers.Should().BeEmpty();
            }

            [Fact]
            public async Task WhenResponseIs401_SetsWWWAuthenticateHeader() {
                _response.StatusCode = 401;
                await _method();
                _response.Headers.Should().ContainKey("WWW-Authenticate");
                _response.Headers["WWW-Authenticate"].Should().BeEquivalentTo("TestScheme realm=\"UnitTests\"");
            }

            [Fact]
            public async Task WhenResponseIs401_AndAHeaderIsAlreadyPresent_AppendsWWWAuthenticateHeader() {
                _response.StatusCode = 401;
                _response.Headers.Add("WWW-Authenticate", new[] {"Bearer"});
                await _method();
                _response.Headers.Should().ContainKey("WWW-Authenticate");
                _response.Headers["WWW-Authenticate"].Should().BeEquivalentTo("Bearer, TestScheme realm=\"UnitTests\"");
            }
        }
    }
}