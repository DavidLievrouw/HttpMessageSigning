#if NETCORE
using System;
using System.Net.Http;
using System.Net.Mime;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.Signing;
using Dalion.HttpMessageSigning.TestUtils;
using Dalion.HttpMessageSigning.Verification;
using Dalion.HttpMessageSigning.Verification.AspNetCore;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Dalion.HttpMessageSigning.QueryString {
    public class QueryString : IDisposable {
        private readonly ITestOutputHelper _output;
        private readonly ServiceProvider _serviceProvider;
        private readonly IRequestSignerFactory _requestSignerFactory;
        private readonly IRequestSignatureVerifier _verifier;
        private readonly SignedRequestAuthenticationOptions _authenticationOptions;

        public QueryString(ITestOutputHelper output) {
            _output = output;
            _serviceProvider = new ServiceCollection().Configure(ConfigureServices).BuildServiceProvider();
            _requestSignerFactory = _serviceProvider.GetRequiredService<IRequestSignerFactory>();
            _verifier = _serviceProvider.GetRequiredService<IRequestSignatureVerifier>();
            _authenticationOptions = new SignedRequestAuthenticationOptions();
        }

        public void Dispose() {
            _serviceProvider?.Dispose();
            _requestSignerFactory?.Dispose();
            _verifier?.Dispose();
        }

        [Fact]
        public async Task CanVerifyRequestContainingQueryString() {
            var request = new HttpRequestMessage {
                RequestUri = new Uri("https://httpbin.org/post?hasQueryString=true"),
                Method = HttpMethod.Post,
                Content = new StringContent("{'id':42}", Encoding.UTF8, MediaTypeNames.Application.Json),
                Headers = {
                    {"Dalion-App-Id", "ringor"}
                }
            };
            
            var requestSigner = _requestSignerFactory.CreateFor("e0e8dcd638334c409e1b88daf821d135");
            await requestSigner.Sign(request);

            var receivedRequest = await request.ToServerSideHttpRequest();

            var verificationResult = await _verifier.VerifySignature(receivedRequest, _authenticationOptions);
            if (verificationResult is RequestSignatureVerificationResultSuccess successResult) {
                var queryString = ExtractQueryStringFromUri(new Uri(successResult.RequestForVerification.RequestUri, UriKind.Relative));
                _output.WriteLine("Verified query string: {0}", queryString);
                queryString.Should().Be("?hasQueryString=true");
            }
            else if (verificationResult is RequestSignatureVerificationResultFailure failureResult) {
                _output.WriteLine("Request signature verification failed: {0}", failureResult.Failure);
                throw new SignatureVerificationException(failureResult.Failure.ToString());
            }
        }

        [Fact]
        public async Task CanVerifyRequestContainingRFC2396EscapedQueryString() {
            var request = new HttpRequestMessage {
                RequestUri = new Uri("https://httpbin.org/post?query+string=%7Bbrooks%7D"),
                Method = HttpMethod.Post,
                Content = new StringContent("{'id':42}", Encoding.UTF8, MediaTypeNames.Application.Json),
                Headers = {
                    {"Dalion-App-Id", "ringor"}
                }
            };
            
            var requestSigner = _requestSignerFactory.CreateFor("e0e8dcd638334c409e1b88daf821d135");
            await requestSigner.Sign(request);

            var receivedRequest = await request.ToServerSideHttpRequest();

            var verificationResult = await _verifier.VerifySignature(receivedRequest, _authenticationOptions);
            if (verificationResult is RequestSignatureVerificationResultSuccess successResult) {
                var queryString = ExtractQueryStringFromUri(new Uri(successResult.RequestForVerification.RequestUri, UriKind.Relative));
                _output.WriteLine("Verified query string: {0}", queryString);
                queryString.Should().Be("?query%2Bstring=%7Bbrooks%7D");
            }
            else if (verificationResult is RequestSignatureVerificationResultFailure failureResult) {
                _output.WriteLine("Request signature verification failed: {0}", failureResult.Failure);
                throw new SignatureVerificationException(failureResult.Failure.ToString());
            }
        }

        [Fact]
        public async Task CanVerifyRequestContainingRFC3986EscapedQueryString() {
            var request = new HttpRequestMessage {
                RequestUri = new Uri("https://httpbin.org/post?query%2Bstring=%7Bbrooks%7D"),
                Method = HttpMethod.Post,
                Content = new StringContent("{'id':42}", Encoding.UTF8, MediaTypeNames.Application.Json),
                Headers = {
                    {"Dalion-App-Id", "ringor"}
                }
            };
            
            var requestSigner = _requestSignerFactory.CreateFor("e0e8dcd638334c409e1b88daf821d135");
            await requestSigner.Sign(request);

            var receivedRequest = await request.ToServerSideHttpRequest();

            var verificationResult = await _verifier.VerifySignature(receivedRequest, _authenticationOptions);
            if (verificationResult is RequestSignatureVerificationResultSuccess successResult) {
                var queryString = ExtractQueryStringFromUri(new Uri(successResult.RequestForVerification.RequestUri, UriKind.Relative));
                _output.WriteLine("Verified query string: {0}", queryString);
                queryString.Should().Be("?query%2Bstring=%7Bbrooks%7D");
            }
            else if (verificationResult is RequestSignatureVerificationResultFailure failureResult) {
                _output.WriteLine("Request signature verification failed: {0}", failureResult.Failure);
                throw new SignatureVerificationException(failureResult.Failure.ToString());
            }
        }
        
        [Fact]
        public async Task CanVerifyRequestsWithoutQueryString() {
            var request = new HttpRequestMessage {
                RequestUri = new Uri("https://httpbin.org/post"),
                Method = HttpMethod.Post,
                Content = new StringContent("{'id':42}", Encoding.UTF8, MediaTypeNames.Application.Json),
                Headers = {
                    {"Dalion-App-Id", "ringor"}
                }
            };
            
            var requestSigner = _requestSignerFactory.CreateFor("e0e8dcd638334c409e1b88daf821d135");
            await requestSigner.Sign(request);

            var receivedRequest = await request.ToServerSideHttpRequest();

            var verificationResult = await _verifier.VerifySignature(receivedRequest, _authenticationOptions);
            if (verificationResult is RequestSignatureVerificationResultSuccess successResult) {
                var queryString = ExtractQueryStringFromUri(new Uri(successResult.RequestForVerification.RequestUri, UriKind.Relative));
                _output.WriteLine("Verified query string: {0}", queryString);
                queryString.Should().BeEmpty();
            }
            else if (verificationResult is RequestSignatureVerificationResultFailure failureResult) {
                _output.WriteLine("Request signature verification failed: {0}", failureResult.Failure);
                throw new SignatureVerificationException(failureResult.Failure.ToString());
            }
        }

        [Fact]
        public async Task VerificationFailsWhenQueryStringIsMissing() {
            var request = new HttpRequestMessage {
                RequestUri = new Uri("https://httpbin.org/post?hasQueryString=true"),
                Method = HttpMethod.Post,
                Content = new StringContent("{'id':42}", Encoding.UTF8, MediaTypeNames.Application.Json),
                Headers = {
                    {"Dalion-App-Id", "ringor"}
                }
            };

            _authenticationOptions.OnSignatureParsed = (req, signature) => {
                req.QueryString = Microsoft.AspNetCore.Http.QueryString.Empty; // Exclude received query string from verification
                return Task.CompletedTask;
            };
            
            var requestSigner = _requestSignerFactory.CreateFor("e0e8dcd638334c409e1b88daf821d135");
            await requestSigner.Sign(request);

            var receivedRequest = await request.ToServerSideHttpRequest();

            var verificationResult = await _verifier.VerifySignature(receivedRequest, _authenticationOptions);
            verificationResult.Should().BeAssignableTo<RequestSignatureVerificationResultFailure>();
            _output.WriteLine("Request signature verification failed: {0}", verificationResult.As<RequestSignatureVerificationResultFailure>().Failure);
        }

        private void ConfigureServices(IServiceCollection services) {
            services
                .AddHttpMessageSigning(
                    new KeyId("e0e8dcd638334c409e1b88daf821d135"),
                    provider => new SigningSettings {
                        SignatureAlgorithm = SignatureAlgorithm.CreateForSigning("yumACY64r%hm"),
                        DigestHashAlgorithm = HashAlgorithmName.SHA256,
                        Expires = TimeSpan.FromMinutes(1),
                        Headers = new[] {
                            (HeaderName) "Dalion-App-Id"
                        }
                    })
                .AddHttpMessageSignatureVerification(provider => {
                    var clientStore = new InMemoryClientStore();
                    clientStore.Register(new Client(
                        new KeyId("e0e8dcd638334c409e1b88daf821d135"),
                        "HttpMessageSigningSampleHMAC",
                        SignatureAlgorithm.CreateForVerification("yumACY64r%hm"),
                        TimeSpan.FromMinutes(5),
                        TimeSpan.FromMinutes(1),
                        new Claim(SignedHttpRequestClaimTypes.Role, "users.read")));
                    return clientStore;
                });
        }

        private static string ExtractQueryStringFromUri(Uri uri) {
            if (uri.IsAbsoluteUri) {
                return uri.Query;
            }

            var originalString = uri.OriginalString;
            var idx = originalString.IndexOf('?');
            var query = idx >= 0 ? originalString.Substring(idx) : "";
            return query;
        }
    }
}
#endif