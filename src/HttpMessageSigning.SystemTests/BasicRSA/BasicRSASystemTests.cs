#if !NETFRAMEWORK
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.Signing;
using Dalion.HttpMessageSigning.TestUtils;
using Dalion.HttpMessageSigning.Verification;
using Dalion.HttpMessageSigning.Verification.AspNetCore;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Dalion.HttpMessageSigning.BasicRSA {
    public class BasicRSASystemTests : IDisposable {
        private readonly ITestOutputHelper _output;
        private readonly ServiceProvider _serviceProvider;
        private readonly IRequestSignerFactory _requestSignerFactory;
        private readonly IRequestSignatureVerifier _verifier;
        private readonly SignedRequestAuthenticationOptions _options;

        public BasicRSASystemTests(ITestOutputHelper output) {
            _output = output;
            _serviceProvider = new ServiceCollection().Configure(ConfigureServices).BuildServiceProvider();
            _requestSignerFactory = _serviceProvider.GetRequiredService<IRequestSignerFactory>();
            _verifier = _serviceProvider.GetRequiredService<IRequestSignatureVerifier>();
            _options = new SignedRequestAuthenticationOptions();
        }

        public void Dispose() {
            _serviceProvider?.Dispose();
            _requestSignerFactory?.Dispose();
            _verifier?.Dispose();
        }

        [Fact]
        public async Task CreatesSignatureThatCanBeValidated() {
            var request = new HttpRequestMessage {
                RequestUri = new Uri("https://httpbin.org/post"),
                Method = HttpMethod.Post,
                Content = new StringContent("{'id':42}", Encoding.UTF8, MediaTypeNames.Application.Json),
                Headers = {
                    {"Dalion-App-Id", "ringor"}
                }
            };
            
            var requestSigner = _requestSignerFactory.CreateFor("4d8f14b6c4184dc1b677c88a2b60bfd2");
            await requestSigner.Sign(request);

            var receivedRequest = await request.ToServerSideHttpRequest();

            var verificationResult = await _verifier.VerifySignature(receivedRequest, _options);
            if (verificationResult is RequestSignatureVerificationResultSuccess successResult) {
                var simpleClaims = successResult.Principal.Claims.Select(c => new {c.Type, c.Value}).ToList();
                var claimsString = string.Join(", ", simpleClaims.Select(c => $"{{type:{c.Type},value:{c.Value}}}"));
                _output.WriteLine("Request signature verification succeeded: {0}", claimsString);
            }
            else if (verificationResult is RequestSignatureVerificationResultFailure failureResult) {
                _output.WriteLine("Request signature verification failed: {0}", failureResult.Failure);
                throw new SignatureVerificationException(failureResult.Failure.ToString());
            }
        }        
        
        [Fact]
        public async Task SupportsRelativeUris() {
            var request = new HttpRequestMessage {
                RequestUri = new Uri("https://httpbin.org/post?id=42"),
                Method = HttpMethod.Post,
                Content = new StringContent("{'id':42}", Encoding.UTF8, MediaTypeNames.Application.Json),
                Headers = {
                    {"Dalion-App-Id", "ringor"}
                }
            };
            
            var requestSigner = _requestSignerFactory.CreateFor("4d8f14b6c4184dc1b677c88a2b60bfd2");
            await requestSigner.Sign(request);

            var receivedRequest = await request.ToServerSideHttpRequest();

            var verificationResult = await _verifier.VerifySignature(receivedRequest, _options);
            if (verificationResult is RequestSignatureVerificationResultSuccess successResult) {
                var simpleClaims = successResult.Principal.Claims.Select(c => new {c.Type, c.Value}).ToList();
                var claimsString = string.Join(", ", simpleClaims.Select(c => $"{{type:{c.Type},value:{c.Value}}}"));
                _output.WriteLine("Request signature verification succeeded: {0}", claimsString);
            }
            else if (verificationResult is RequestSignatureVerificationResultFailure failureResult) {
                _output.WriteLine("Request signature verification failed: {0}", failureResult.Failure);
                throw new SignatureVerificationException(failureResult.Failure.ToString());
            }
        }
        
        [Fact]
        public async Task SupportsRFC2396EscapedUris() {
            var request = new HttpRequestMessage {
                RequestUri = new Uri("https://httpbin.org/post/David%20&%20Partners%20+%20Siebe%20at%20100%25%20*%20co.?query+string=%7Bbrooks%7D"),
                Method = HttpMethod.Post,
                Content = new StringContent("{'id':42}", Encoding.UTF8, MediaTypeNames.Application.Json),
                Headers = {
                    {"Dalion-App-Id", "ringor"}
                }
            };
            
            var requestSigner = _requestSignerFactory.CreateFor("4d8f14b6c4184dc1b677c88a2b60bfd2");
            await requestSigner.Sign(request);

            var receivedRequest = await request.ToServerSideHttpRequest();

            var verificationResult = await _verifier.VerifySignature(receivedRequest, _options);
            if (verificationResult is RequestSignatureVerificationResultSuccess successResult) {
                var simpleClaims = successResult.Principal.Claims.Select(c => new {c.Type, c.Value}).ToList();
                var claimsString = string.Join(", ", simpleClaims.Select(c => $"{{type:{c.Type},value:{c.Value}}}"));
                _output.WriteLine("Request signature verification succeeded: {0}", claimsString);
            }
            else if (verificationResult is RequestSignatureVerificationResultFailure failureResult) {
                _output.WriteLine("Request signature verification failed: {0}", failureResult.Failure);
                throw new SignatureVerificationException(failureResult.Failure.ToString());
            }
        }
        
        [Fact]
        public async Task SupportsRFC3986EscapedUris() {
            var request = new HttpRequestMessage {
                RequestUri = new Uri("https://httpbin.org/post/David%20%26%20Partners%20%2B%20Siebe%20at%20100%25%20%2A%20co.?query%2Bstring=%7Bbrooks%7D"),
                Method = HttpMethod.Post,
                Content = new StringContent("{'id':42}", Encoding.UTF8, MediaTypeNames.Application.Json),
                Headers = {
                    {"Dalion-App-Id", "ringor"}
                }
            };
            
            var requestSigner = _requestSignerFactory.CreateFor("4d8f14b6c4184dc1b677c88a2b60bfd2");
            await requestSigner.Sign(request);

            var receivedRequest = await request.ToServerSideHttpRequest();

            var verificationResult = await _verifier.VerifySignature(receivedRequest, _options);
            if (verificationResult is RequestSignatureVerificationResultSuccess successResult) {
                var simpleClaims = successResult.Principal.Claims.Select(c => new {c.Type, c.Value}).ToList();
                var claimsString = string.Join(", ", simpleClaims.Select(c => $"{{type:{c.Type},value:{c.Value}}}"));
                _output.WriteLine("Request signature verification succeeded: {0}", claimsString);
            }
            else if (verificationResult is RequestSignatureVerificationResultFailure failureResult) {
                _output.WriteLine("Request signature verification failed: {0}", failureResult.Failure);
                throw new SignatureVerificationException(failureResult.Failure.ToString());
            }
        }
        
        [Fact]
        public async Task SupportsPartiallyEscapedUris() {
            var request = new HttpRequestMessage {
                RequestUri = new Uri("https://httpbin.org/{Brooks} was here/create/David%20&%20Partners%20+%20Siebe%20at%20100%25%20*%20co."),
                Method = HttpMethod.Post,
                Content = new StringContent("{'id':42}", Encoding.UTF8, MediaTypeNames.Application.Json),
                Headers = {
                    {"Dalion-App-Id", "ringor"}
                }
            };
            
            var requestSigner = _requestSignerFactory.CreateFor("4d8f14b6c4184dc1b677c88a2b60bfd2");
            await requestSigner.Sign(request);

            var receivedRequest = await request.ToServerSideHttpRequest();

            var verificationResult = await _verifier.VerifySignature(receivedRequest, _options);
            if (verificationResult is RequestSignatureVerificationResultSuccess successResult) {
                var simpleClaims = successResult.Principal.Claims.Select(c => new {c.Type, c.Value}).ToList();
                var claimsString = string.Join(", ", simpleClaims.Select(c => $"{{type:{c.Type},value:{c.Value}}}"));
                _output.WriteLine("Request signature verification succeeded: {0}", claimsString);
            }
            else if (verificationResult is RequestSignatureVerificationResultFailure failureResult) {
                _output.WriteLine("Request signature verification failed: {0}", failureResult.Failure);
                throw new SignatureVerificationException(failureResult.Failure.ToString());
            }
        }
        
        [Fact]
        public async Task SupportsUnescapedUris() {
            var request = new HttpRequestMessage {
                RequestUri = new Uri("https://httpbin.org/{Brooks} was here/create/David & Partners + Siebe at 100% * co.?query+string={brooks}"),
                Method = HttpMethod.Post,
                Content = new StringContent("{'id':42}", Encoding.UTF8, MediaTypeNames.Application.Json),
                Headers = {
                    {"Dalion-App-Id", "ringor"}
                }
            };
            
            var requestSigner = _requestSignerFactory.CreateFor("4d8f14b6c4184dc1b677c88a2b60bfd2");
            await requestSigner.Sign(request);

            var receivedRequest = await request.ToServerSideHttpRequest();

            var verificationResult = await _verifier.VerifySignature(receivedRequest, _options);
            if (verificationResult is RequestSignatureVerificationResultSuccess successResult) {
                var simpleClaims = successResult.Principal.Claims.Select(c => new {c.Type, c.Value}).ToList();
                var claimsString = string.Join(", ", simpleClaims.Select(c => $"{{type:{c.Type},value:{c.Value}}}"));
                _output.WriteLine("Request signature verification succeeded: {0}", claimsString);
            }
            else if (verificationResult is RequestSignatureVerificationResultFailure failureResult) {
                _output.WriteLine("Request signature verification failed: {0}", failureResult.Failure);
                throw new SignatureVerificationException(failureResult.Failure.ToString());
            }
        }

        [Fact]
        public async Task InvalidSignatureString_ReturnsFailure() {
            var request = new HttpRequestMessage {
                RequestUri = new Uri("https://httpbin.org/post"),
                Method = HttpMethod.Post,
                Content = new StringContent("{'id':42}", Encoding.UTF8, MediaTypeNames.Application.Json),
                Headers = {
                    {"Dalion-App-Id", "ringor"}
                }
            };
            
            var requestSigner = _requestSignerFactory.CreateFor("4d8f14b6c4184dc1b677c88a2b60bfd2");
            await requestSigner.Sign(request);
            
            var signatureStringRegEx = new Regex("signature=\"(?<signature>[a-zA-Z0-9+/]+={0,2})\"", RegexOptions.Compiled);
            var match = signatureStringRegEx.Match(request.Headers.Authorization.Parameter);
            request.Headers.Authorization = new AuthenticationHeaderValue(
                request.Headers.Authorization.Scheme,
                request.Headers.Authorization.Parameter.Replace(match.Groups["signature"].Value, "a" + match.Groups["signature"].Value));
            
            var receivedRequest = await request.ToServerSideHttpRequest();

            var verificationResult = await _verifier.VerifySignature(receivedRequest, _options);
            verificationResult.Should().BeAssignableTo<RequestSignatureVerificationResultFailure>();
            _output.WriteLine("Request signature verification failed: {0}", verificationResult.As<RequestSignatureVerificationResultFailure>().Failure);
        }
        
        private static void ConfigureServices(IServiceCollection services) {
            var rsa = new RSACryptoServiceProvider();
            var privateKey = rsa.ExportParameters(includePrivateParameters: true);
            var publicKey = rsa.ExportParameters(includePrivateParameters: false);
            
            services
                .AddHttpMessageSigning()
                .UseKeyId("4d8f14b6c4184dc1b677c88a2b60bfd2")
                .UseSignatureAlgorithm(SignatureAlgorithm.CreateForSigning(privateKey))
                .Services
                .AddHttpMessageSignatureVerification()
                .UseAspNetCoreSignatureVerification()
                .UseClient(Client.Create(
                    "4d8f14b6c4184dc1b677c88a2b60bfd2",
                    "HttpMessageSigningSampleRSA",
                    SignatureAlgorithm.CreateForVerification(publicKey),
                    options => options.Claims = new [] {
                        new Claim(SignedHttpRequestClaimTypes.Role, "users.read")
                    }
                ));
        }
    }
}
#endif