#if NETCORE
using System;
using System.Linq;
using System.Net.Http;
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

namespace Dalion.HttpMessageSigning.CreatedAndExpiresHeaders {
    public class SupportsOptionalCreatedAndExpiresHeaders : IDisposable {
        private readonly ITestOutputHelper _output;
        private readonly ServiceProvider _serviceProvider;
        private readonly IRequestSignerFactory _requestSignerFactory;
        private readonly IRequestSignatureVerifier _verifier;
        private readonly SignedRequestAuthenticationOptions _options;

        public SupportsOptionalCreatedAndExpiresHeaders(ITestOutputHelper output) {
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
            
            var requestSigner = _requestSignerFactory.CreateFor("e0e8dcd638334c409e1b88daf821d135");
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
        public async Task WhenExpiresHeaderDoesNotMatch_ReturnsFailure() {
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
            
            var signatureStringRegEx = new Regex("expires=(?<expiresValue>[0-9]+)", RegexOptions.Compiled);
            var match = signatureStringRegEx.Match(receivedRequest.Headers["Authorization"]);
            receivedRequest.Headers["Authorization"] = receivedRequest.Headers["Authorization"].First()
                .Replace(
                    match.Groups["expiresValue"].Value,
                    (long.Parse(match.Groups["expiresValue"].Value) - 1).ToString());

            var verificationResult = await _verifier.VerifySignature(receivedRequest, _options);
            verificationResult.Should().BeAssignableTo<RequestSignatureVerificationResultFailure>();
            _output.WriteLine("Request signature verification failed: {0}", verificationResult.As<RequestSignatureVerificationResultFailure>().Failure);
        }
        
        [Fact]
        public async Task ExpiresIsOptional() {
            var request = new HttpRequestMessage {
                RequestUri = new Uri("https://httpbin.org/post"),
                Method = HttpMethod.Post,
                Content = new StringContent("{'id':42}", Encoding.UTF8, MediaTypeNames.Application.Json),
                Headers = {
                    {"Dalion-App-Id", "ringor"}
                }
            };

            var requestSigner = _requestSignerFactory.Create(
                new KeyId("e0e8dcd638334c409e1b88daf821d135"),
                new SigningSettings {
                    SignatureAlgorithm = SignatureAlgorithm.CreateForSigning("yumACY64r%hm"),
                    DigestHashAlgorithm = HashAlgorithmName.SHA256,
                    Expires = TimeSpan.FromMinutes(1),
                    Headers = new[] {
                        (HeaderName) "Dalion-App-Id",
                        HeaderName.PredefinedHeaderNames.Date,
                        HeaderName.PredefinedHeaderNames.Created,
                        HeaderName.PredefinedHeaderNames.Expires
                    },
                    Events = {
                        OnSigningStringComposed = (HttpRequestMessage requestToSign, ref string signingString) => {
                            var signingStringRegEx = new Regex(@"\(expires\): ([0-9]+)\n", RegexOptions.Compiled);
                            signingString = signingStringRegEx.Replace(signingString, string.Empty); // Remove (expires) from signing string
                            return Task.CompletedTask;
                        },
                        OnSignatureCreated = (requestToSign, signature, settings) => {
                            // Exclude expires header and value
                            signature.Expires = null; 
                            signature.Headers = signature.Headers.Where(_ => _ != HeaderName.PredefinedHeaderNames.Expires).ToArray();
                            return Task.CompletedTask;
                        }
                    }
                });
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
        
        private static void ConfigureServices(IServiceCollection services) {
            services
                .AddHttpMessageSigning()
                .UseKeyId("e0e8dcd638334c409e1b88daf821d135")
                .UseSignatureAlgorithm(SignatureAlgorithm.CreateForSigning("yumACY64r%hm"))
                .UseDigestAlgorithm(HashAlgorithmName.SHA256)
                .UseExpires(TimeSpan.FromMinutes(1))
                .UseHeaders(
                    (HeaderName)"Dalion-App-Id",
                    HeaderName.PredefinedHeaderNames.Date,
                    HeaderName.PredefinedHeaderNames.Created,
                    HeaderName.PredefinedHeaderNames.Expires)
                .Services
                .AddHttpMessageSignatureVerification()
                .UseAspNetCoreSignatureVerification()
                .UseClient(Client.Create(
                    "e0e8dcd638334c409e1b88daf821d135",
                    "HttpMessageSigningSampleHMAC",
                    SignatureAlgorithm.CreateForVerification("yumACY64r%hm"),
                    options => options.Claims = new [] {
                        new Claim(SignedHttpRequestClaimTypes.Role, "users.read")
                    }
                ));
        }
    }
}
#endif