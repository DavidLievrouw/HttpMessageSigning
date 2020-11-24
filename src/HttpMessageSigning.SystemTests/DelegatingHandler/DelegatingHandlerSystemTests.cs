#if NETCORE
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.Signing;
using Dalion.HttpMessageSigning.TestUtils;
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
        private readonly SignedRequestAuthenticationOptions _options;
        
        private HttpRequestMessage _signedRequest;
        private string _signatureString;

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
        public async Task CanHandleRFC2396EscapedUris() {
            var uri = new Uri("/anything/David%20&%20Partners%20+%20Siebe%20at%20100%25%20*%20co.?query+string=%7Bbrooks%7D", UriKind.Relative);

            var response = await _senderService.SendTo(uri);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            _signatureString.Should().Contain("(request-target): post /anything/David%20%26%20Partners%20%2B%20Siebe%20at%20100%25%20%2A%20co.?query%2Bstring=%7Bbrooks%7D");
            
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
        public async Task CanHandleUnescapedUris() {
            var uri = new Uri("/anything/David & Partners + Siebe at 100% * co.?query+string={brooks}", UriKind.Relative);

            var response = await _senderService.SendTo(uri);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            _signatureString.Should().Contain("(request-target): post /anything/David%20%26%20Partners%20%2B%20Siebe%20at%20100%25%20%2A%20co.?query%2Bstring=%7Bbrooks%7D");
            
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
        public async Task CanHandleRFC3986EscapedUris() {
            var uri = new Uri("/anything/David%20%26%20Partners%20%2B%20Siebe%20at%20100%25%20%2A%20co.?query%2Bstring=%7Bbrooks%7D", UriKind.Relative);

            var response = await _senderService.SendTo(uri);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            _signatureString.Should().Contain("(request-target): post /anything/David%20%26%20Partners%20%2B%20Siebe%20at%20100%25%20%2A%20co.?query%2Bstring=%7Bbrooks%7D");
            
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
                .AddHttpMessageSigning()
                .UseKeyId(keyId)
                .UseSignatureAlgorithm(SignatureAlgorithm.CreateForSigning("yumACY64r%hm"))
                .UseDigestAlgorithm(HashAlgorithmName.SHA256)
                .UseExpires(TimeSpan.FromMinutes(1))
                .UseHeaders((HeaderName)"Dalion-App-Id")
                .UseOnSigningStringComposedEvent(OnSigningStringComposed)
                .UseOnRequestSignedEvent(OnRequestSigned)
                .Services
                .AddHttpMessageSignatureVerification(provider => {
                    var clientStore = new InMemoryClientStore();
                    clientStore.Register(new Client(
                        keyId,
                        "HttpMessageSigningSampleHMAC",
                        SignatureAlgorithm.CreateForVerification("yumACY64r%hm"),
                        TimeSpan.FromMinutes(5),
                        TimeSpan.FromMinutes(1),
                        RequestTargetEscaping.RFC3986,
                        new Claim(SignedHttpRequestClaimTypes.Role, "users.read")));
                    return clientStore;
                })
                .AddHttpClient<SenderService>(config => config.BaseAddress = new Uri("https://httpbin.org"))
                .AddHttpMessageHandler(provider => new HttpRequestSigningHandler(provider.GetRequiredService<IRequestSignerFactory>().CreateFor(keyId)))
                .AddHttpMessageHandler(() => new FakeDelegatingHandler(new HttpResponseMessage(HttpStatusCode.Created)))
                .Services
                .AddTransient<HttpRequestSigningHandler>();
        }

        private Task OnSigningStringComposed(HttpRequestMessage request, ref string signatureString) {
            _signatureString = signatureString;
            return Task.CompletedTask;
        }

        private Task OnRequestSigned(HttpRequestMessage request, Signature signature, SigningSettings settings) {
            _signedRequest = request;
            return Task.CompletedTask;
        }
    }
}
#endif