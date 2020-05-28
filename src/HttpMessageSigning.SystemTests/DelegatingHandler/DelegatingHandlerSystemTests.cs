#if NETCORE
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.Signing;
using Dalion.HttpMessageSigning.Verification;
using Dalion.HttpMessageSigning.Verification.AspNetCore;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Dalion.HttpMessageSigning.DelegatingHandler {
    public class DelegatingHandlerSystemTests : IDisposable {
        private readonly ITestOutputHelper _output;
        private readonly ServiceProvider _serviceProvider;
        private readonly IRequestSignerFactory _requestSignerFactory;
        private readonly IRequestSignatureVerifier _verifier;
        private readonly SenderService _senderService;
        private HttpRequestMessage _signedRequest;
        private readonly SignedRequestAuthenticationOptions _options;

        public DelegatingHandlerSystemTests(ITestOutputHelper output) {
            _output = output;
            _serviceProvider = new ServiceCollection().Configure(ConfigureServices).BuildServiceProvider();
            _requestSignerFactory = _serviceProvider.GetRequiredService<IRequestSignerFactory>();
            _verifier = _serviceProvider.GetRequiredService<IRequestSignatureVerifier>();
            _senderService = _serviceProvider.GetRequiredService<SenderService>();
            _options = new SignedRequestAuthenticationOptions();
        }

        public void Dispose() {
            _serviceProvider?.Dispose();
            _requestSignerFactory?.Dispose();
            _verifier?.Dispose();
        }

        [Fact]
        public async Task GivenAbsoluteUri_Signs_BeforeSending() {
            var uri = new Uri("https://httpbin.org/post");

            var response = await _senderService.SendTo(uri);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            _signedRequest.RequestUri.Should().Be(uri);
            
            var receivedRequest = await _signedRequest.ToServerSideHttpRequest();

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
        public async Task GivenRelativeUri_SignsWithAbsoluteUri_BeforeSending() {
            var uri = new Uri("/post", UriKind.Relative);

            var response = await _senderService.SendTo(uri);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            _signedRequest.RequestUri.Should().Be(new Uri("https://httpbin.org/post"));
            
            var receivedRequest = await _signedRequest.ToServerSideHttpRequest();

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
        public async Task CanHandleEncodedUris() {
            var uri = new Uri("/anything/David%20%26%20Partners%20%2B%20Siebe%20at%20100%25%20%2A%20co.", UriKind.Relative);

            var response = await _senderService.SendTo(uri);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            _signedRequest.RequestUri.Should().Be(new Uri("https://httpbin.org/anything/David%20%26%20Partners%20%2B%20Siebe%20at%20100%25%20%2A%20co."));
            
            var receivedRequest = await _signedRequest.ToServerSideHttpRequest();

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
        
        private void ConfigureServices(IServiceCollection services) {
            var keyId = new KeyId("e0e8dcd638334c409e1b88daf821d135");
            
            services
                .AddHttpMessageSigning(
                    keyId,
                    provider => new SigningSettings {
                        SignatureAlgorithm = SignatureAlgorithm.CreateForSigning("yumACY64r%hm"),
                        DigestHashAlgorithm = HashAlgorithmName.SHA256,
                        Expires = TimeSpan.FromMinutes(1),
                        Headers = new[] {
                            (HeaderName) "Dalion-App-Id"
                        },
                        Events = new RequestSigningEvents {
                            OnRequestSigned = OnRequestSigned
                        }
                    })
                .AddHttpMessageSignatureVerification(provider => {
                    var clientStore = new InMemoryClientStore();
                    clientStore.Register(new Client(
                        keyId,
                        "HttpMessageSigningSampleHMAC",
                        SignatureAlgorithm.CreateForVerification("yumACY64r%hm"),
                        TimeSpan.FromMinutes(5),
                        TimeSpan.FromMinutes(1),
                        new Claim(SignedHttpRequestClaimTypes.Role, "users.read")));
                    return clientStore;
                })
                .AddHttpClient<SenderService>(config => config.BaseAddress = new Uri("https://httpbin.org"))
                .AddHttpMessageHandler(provider => new HttpRequestSigningHandler(provider.GetRequiredService<IRequestSignerFactory>().CreateFor(keyId)))
                .AddHttpMessageHandler(() => new FakeDelegatingHandler(new HttpResponseMessage(HttpStatusCode.Created)))
                .Services
                .AddTransient<HttpRequestSigningHandler>();
        }

        private Task OnRequestSigned(HttpRequestMessage request, Signature signature, SigningSettings settings) {
            _signedRequest = request;
            return Task.CompletedTask;
        }
    }
}
#endif