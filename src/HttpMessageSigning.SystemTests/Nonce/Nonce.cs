#if NETCORE
using System;
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

namespace Dalion.HttpMessageSigning.Nonce {
    public class NonceSystemTests : IDisposable {
        private readonly ServiceProvider _serviceProvider;
        private readonly IRequestSignerFactory _requestSignerFactory;
        private readonly IRequestSignatureVerifier _verifier;
        private bool _nonceEnabled;
        private readonly SignedRequestAuthenticationOptions _authenticationOptions;

        public NonceSystemTests() {
            _serviceProvider = new ServiceCollection().Configure(ConfigureServices).BuildServiceProvider();
            _requestSignerFactory = _serviceProvider.GetRequiredService<IRequestSignerFactory>();
            _verifier = _serviceProvider.GetRequiredService<IRequestSignatureVerifier>();
            _nonceEnabled = true;
            _authenticationOptions = new SignedRequestAuthenticationOptions();
        }

        public void Dispose() {
            _serviceProvider?.Dispose();
            _requestSignerFactory?.Dispose();
            _verifier?.Dispose();
        }

        [Fact]
        public async Task WhenNonceIsEnabled_BlocksReplayedRequest() {
            _nonceEnabled = true;
            
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

            var verificationResult1 = await _verifier.VerifySignature(receivedRequest, _authenticationOptions);
            verificationResult1.Should().BeAssignableTo<RequestSignatureVerificationResultSuccess>();
            
            var verificationResult2 = await _verifier.VerifySignature(receivedRequest, _authenticationOptions);
            verificationResult2.Should().BeAssignableTo<RequestSignatureVerificationResultFailure>();
            verificationResult2.As<RequestSignatureVerificationResultFailure>().Failure.Code.Should().Be("REPLAYED_REQUEST");
        }
        
        [Fact]
        public async Task WhenNonceIsDisabled_BlocksReplayedRequest() {
            _nonceEnabled = false;
            
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

            var verificationResult1 = await _verifier.VerifySignature(receivedRequest, _authenticationOptions);
            verificationResult1.Should().BeAssignableTo<RequestSignatureVerificationResultSuccess>();
            
            var verificationResult2 = await _verifier.VerifySignature(receivedRequest, _authenticationOptions);
            verificationResult2.Should().BeAssignableTo<RequestSignatureVerificationResultSuccess>();
        }

        [Fact]
        public async Task WhenNonceIsNotIncludedInAuthParam_ButItIsEnabled_FailsVerification() {
            _nonceEnabled = true;
            
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

            var nonceRegex = new Regex("nonce=\"(?<nonce>[A-z0-9, =-]+)\",", RegexOptions.Compiled);
            var match = nonceRegex.Match(request.Headers.Authorization.Parameter);
            request.Headers.Authorization = new AuthenticationHeaderValue(
                request.Headers.Authorization.Scheme,
                request.Headers.Authorization.Parameter.Replace(match.Value, ""));
            
            var receivedRequest = await request.ToServerSideHttpRequest();

            var verificationResult = await _verifier.VerifySignature(receivedRequest, _authenticationOptions);
            verificationResult.Should().BeAssignableTo<RequestSignatureVerificationResultFailure>();
            verificationResult.As<RequestSignatureVerificationResultFailure>().Failure.Code.Should().Be("INVALID_SIGNATURE_STRING");
        }

        [Fact]
        public async Task WhenNonceIsModified_FailsVerification() {
            _nonceEnabled = true;
            
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

            var nonceRegex = new Regex("nonce=\"(?<nonce>[A-z0-9, =-]+)\"", RegexOptions.Compiled);
            var match = nonceRegex.Match(request.Headers.Authorization.Parameter);
            request.Headers.Authorization = new AuthenticationHeaderValue(
                request.Headers.Authorization.Scheme,
                request.Headers.Authorization.Parameter.Replace(match.Groups["nonce"].Value, "a" + match.Groups["nonce"].Value));
            
            var receivedRequest = await request.ToServerSideHttpRequest();

            var verificationResult = await _verifier.VerifySignature(receivedRequest, _authenticationOptions);
            verificationResult.Should().BeAssignableTo<RequestSignatureVerificationResultFailure>();
            verificationResult.As<RequestSignatureVerificationResultFailure>().Failure.Code.Should().Be("INVALID_SIGNATURE_STRING");
        }
        
        private void UpdateNonceEnabled(SigningSettings settings) {
            settings.EnableNonce = _nonceEnabled;
        }
        
        private void ConfigureServices(IServiceCollection services) {
            services                
                .AddHttpMessageSigning()
                .UseKeyId("e0e8dcd638334c409e1b88daf821d135")
                .UseSignatureAlgorithm(SignatureAlgorithm.CreateForSigning("yumACY64r%hm"))
                .UseDigestAlgorithm(HashAlgorithmName.SHA256)
                .UseExpires(TimeSpan.FromMinutes(1))
                .UseHeaders((HeaderName)"Dalion-App-Id")
                .UseOnRequestSigningEvent((message, settings) => {
                    UpdateNonceEnabled(settings);
                    return Task.CompletedTask;
                })
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