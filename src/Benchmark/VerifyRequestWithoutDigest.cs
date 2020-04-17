using System;
using System.Net.Http;
using System.Net.Mime;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using Dalion.HttpMessageSigning;
using Dalion.HttpMessageSigning.Signing;
using Dalion.HttpMessageSigning.Verification;
using Dalion.HttpMessageSigning.Verification.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Benchmark {
    [Config(typeof(Config))]
    public class VerifyRequestWithoutDigest {
        private readonly IRequestSignatureVerifier _verifier;
        private readonly HttpRequest _request;

        public VerifyRequestWithoutDigest() {
            var keyId = new KeyId("e0e8dcd638334c409e1b88daf821d135");
            var serviceProvider = new ServiceCollection()
                .AddHttpMessageSigning(
                    keyId,
                    provider => new SigningSettings {
                        SignatureAlgorithm = SignatureAlgorithm.CreateForSigning("yumACY64r%hm"),
                        DigestHashAlgorithm = default,
                        EnableNonce = false,
                        Expires = TimeSpan.FromMinutes(1),
                        Headers = new [] {
                            (HeaderName)"Dalion-App-Id"
                        }
                    })
                .AddHttpMessageSignatureVerification(provider => {
                    var clientStore = new InMemoryClientStore();
                    clientStore.Register(new Client(
                        new KeyId("e0e8dcd638334c409e1b88daf821d135"),
                        "HttpMessageSigningSampleHMAC",
                        SignatureAlgorithm.CreateForVerification("yumACY64r%hm"),
                        TimeSpan.FromMilliseconds(1),
                        new Claim(SignedHttpRequestClaimTypes.Role, "users.read")));
                    return clientStore;
                })
                .BuildServiceProvider();
            var requestSignerFactory = serviceProvider.GetRequiredService<IRequestSignerFactory>();
            var requestSigner = requestSignerFactory.CreateFor(keyId);
            var request = new HttpRequestMessage {
                RequestUri = new Uri("https://httpbin.org/post"),
                Method = HttpMethod.Post,
                Content = new StringContent("{'id':42}", Encoding.UTF8, MediaTypeNames.Application.Json),
                Headers = {
                    {"Dalion-App-Id", "ringor"}
                }
            };
            requestSigner.Sign(request).GetAwaiter().GetResult();
            _verifier = serviceProvider.GetRequiredService<IRequestSignatureVerifier>();
            _request = request.ToServerSideHttpRequest().GetAwaiter().GetResult();
        }

        [Benchmark]
        public async Task Verify() {
            await _verifier.VerifySignature(_request);
        }
        
        public async Task VerifyABunchOfTimes() {
            for (var i = 0; i < 100000; i++) {
                await _verifier.VerifySignature(_request);
            }
        }
        
        private class Config : ManualConfig {
            public Config() {
                AddDiagnoser(MemoryDiagnoser.Default);
            }
        }
    }
}