using System;
using System.IO;
using System.Net.Http;
using System.Net.Mime;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using Dalion.HttpMessageSigning;
using Dalion.HttpMessageSigning.Signing;
using Dalion.HttpMessageSigning.TestUtils;
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
            var cert = new X509Certificate2(File.ReadAllBytes("./dalion.local.pfx"), "CertP@ss123", X509KeyStorageFlags.Exportable);
            
            var serviceProvider = new ServiceCollection()                
                .AddHttpMessageSigning()
                .UseKeyId(keyId)
                .UseSignatureAlgorithm(SignatureAlgorithm.CreateForSigning("yumACY64r%hm"))
                .UseExpires(TimeSpan.FromMinutes(1))
                .UseHeaders((HeaderName)"Dalion-App-Id")
                .Services
                .AddHttpMessageSignatureVerification()
                .UseClient(Client.Create(
                    "e0e8dcd638334c409e1b88daf821d135",
                    "HttpMessageSigningSampleHMAC",
                    SignatureAlgorithm.CreateForVerification("yumACY64r%hm"),
                    options => options.Claims = new [] {
                        new Claim(SignedHttpRequestClaimTypes.Role, "users.read")
                    }
                ))
                .Services
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
        public async Task<RequestSignatureVerificationResult> Verify() {
            var result = await _verifier.VerifySignature(_request, new SignedRequestAuthenticationOptions());
            return result;
        }
        
        private class Config : ManualConfig {
            public Config() {
                AddDiagnoser(MemoryDiagnoser.Default);
            }
        }
    }
}