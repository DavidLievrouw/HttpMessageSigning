#if !NETFRAMEWORK
using System;
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

namespace Dalion.HttpMessageSigning.ExtensionPoints {
    public class UpdateSigningSettingsBeforeSigning : IDisposable {
        private readonly ITestOutputHelper _output;
        private readonly ServiceProvider _serviceProvider;
        private readonly IRequestSignerFactory _requestSignerFactory;
        private readonly IRequestSignatureVerifier _verifier;
        private readonly SignedRequestAuthenticationOptions _options;

        public UpdateSigningSettingsBeforeSigning(ITestOutputHelper output) {
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
        public async Task CanUpdateSigningSettingsBeforeSigning() {
            var request = new HttpRequestMessage {
                RequestUri = new Uri("https://httpbin.org/post"),
                Method = HttpMethod.Get,
                Headers = {
                    {"Dalion-App-Id", "ringor"}
                }
            };
            
            var requestSigner = _requestSignerFactory.CreateFor((KeyId)"e0e8dcd638334c409e1b88daf821d135");
            await requestSigner.Sign(request);

            request.Headers.Authorization.Parameter.Should().NotContain("nonce");
        }

        private void ConfigureServices(IServiceCollection services) {
            services
                .AddHttpMessageSigning()
                .UseKeyId((KeyId)"e0e8dcd638334c409e1b88daf821d135")
                .UseSignatureAlgorithm(SignatureAlgorithm.CreateForSigning("yumACY64r%hm"))
                .UseDigestAlgorithm(HashAlgorithmName.SHA256)
                .UseExpires(TimeSpan.FromMinutes(1))
                .UseNonce()
                .UseHeaders((HeaderName)"Dalion-App-Id")
                .UseOnRequestSigningEvent(OnRequestSigning)
                .Services
                .AddHttpMessageSignatureVerification()
                .UseAspNetCoreSignatureVerification()
                .UseClient(Client.Create(
                    (KeyId)"e0e8dcd638334c409e1b88daf821d135",
                    "HttpMessageSigningSampleHMAC",
                    SignatureAlgorithm.CreateForVerification("yumACY64r%hm"),
                    options => options.Claims = new [] {
                        new Claim(SignedHttpRequestClaimTypes.Role, "users.read")
                    }
                ));
        }

        private static Task OnRequestSigning(HttpRequestMessage request, SigningSettings settings) {
            settings.EnableNonce = false;
            return Task.CompletedTask;
        }
    }
}
#endif