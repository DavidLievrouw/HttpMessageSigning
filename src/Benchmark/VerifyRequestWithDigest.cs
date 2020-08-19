using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Mime;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
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
    public class VerifyRequestWithDigest {
        private readonly IRequestSignatureVerifier _verifier;
        private readonly HttpRequest _request;

        public VerifyRequestWithDigest() {
            var keyId = new KeyId("e0e8dcd638334c409e1b88daf821d135");
            var cert = new X509Certificate2(File.ReadAllBytes("./dalion.local.pfx"), "CertP@ss123", X509KeyStorageFlags.Exportable);
            
            var serviceProvider = new ServiceCollection()
                .AddHttpMessageSigning(
                    keyId,
                    provider => new SigningSettings {
                        SignatureAlgorithm = SignatureAlgorithm.CreateForSigning("yumACY64r%hm"),
                        DigestHashAlgorithm = HashAlgorithmName.SHA256,
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
                        TimeSpan.FromMinutes(1),
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
            for (var i = 0; i < 10000; i++) {
                await _verifier.VerifySignature(_request, new SignedRequestAuthenticationOptions());
            }
        }
        
        public async Task VerifyABunchOfTimes() {
            var watch = Stopwatch.StartNew();
            for (var i = 0; i < 1000000; i++) {
                await _verifier.VerifySignature(_request, new SignedRequestAuthenticationOptions());
            }
            watch.Stop();
            Console.WriteLine("Elapsed: {0}ms", watch.ElapsedMilliseconds);
        }
        
        private class Config : ManualConfig {
            public Config() {
                AddDiagnoser(MemoryDiagnoser.Default);
            }
        }
    }
}