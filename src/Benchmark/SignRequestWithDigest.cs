using System;
using System.Net.Http;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using Dalion.HttpMessageSigning;
using Dalion.HttpMessageSigning.Signing;
using Microsoft.Extensions.DependencyInjection;

namespace Benchmark {
    [Config(typeof(Config))]
    public class SignRequestWithDigest {
        private readonly IRequestSigner _requestSigner;
        private readonly HttpRequestMessage _request;

        public SignRequestWithDigest() {
            var keyId = new KeyId("e0e8dcd638334c409e1b88daf821d135");
            var serviceProvider = new ServiceCollection()
                .AddHttpMessageSigning(
                    keyId,
                    provider => new SigningSettings {
                        SignatureAlgorithm = SignatureAlgorithm.CreateForSigning("yumACY64r%hm"),
                        DigestHashAlgorithm = HashAlgorithmName.SHA256,
                        EnableNonce = true,
                        Expires = TimeSpan.FromMinutes(1),
                        Headers = new [] {
                            (HeaderName)"Dalion-App-Id"
                        }
                    })
                .BuildServiceProvider();
            var requestSignerFactory = serviceProvider.GetRequiredService<IRequestSignerFactory>();
            _requestSigner = requestSignerFactory.CreateFor(keyId);
            _request = new HttpRequestMessage {
                RequestUri = new Uri("https://httpbin.org/post"),
                Method = HttpMethod.Post,
                Content = new StringContent("{'id':42}", Encoding.UTF8, MediaTypeNames.Application.Json),
                Headers = {
                    {"Dalion-App-Id", "ringor"}
                }
            };
        }

        [Benchmark]
        public async Task Sign() {
            await _requestSigner.Sign(_request);
        }
        
        public async Task SignAThousandTimes() {
            for (var i = 0; i < 1000; i++) {
                await _requestSigner.Sign(_request);
            }
        }
        
        private class Config : ManualConfig {
            public Config() {
                AddDiagnoser(MemoryDiagnoser.Default);
            }
        }
    }
}