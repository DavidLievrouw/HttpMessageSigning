using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
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
    public class SignRequestWithoutDigest {
        private readonly IRequestSigner _requestSigner;
        private readonly HttpRequestMessage _request;

        public SignRequestWithoutDigest() {
            var keyId = new KeyId("e0e8dcd638334c409e1b88daf821d135");
            var cert = new X509Certificate2(File.ReadAllBytes("./dalion.local.pfx"), "CertP@ss123", X509KeyStorageFlags.Exportable);
            
            var serviceProvider = new ServiceCollection()
                .AddHttpMessageSigning()
                .UseKeyId(keyId)
                .UseSignatureAlgorithm(SignatureAlgorithm.CreateForSigning(cert, HashAlgorithmName.SHA256))
                .UseExpires(TimeSpan.FromMinutes(1))
                .UseHeaders((HeaderName)"Dalion-App-Id")
                .Services
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
        public async Task<HttpRequestMessage> Sign() {
            await _requestSigner.Sign(_request);
            return _request;
        }
        
        private class Config : ManualConfig {
            public Config() {
                AddDiagnoser(MemoryDiagnoser.Default);
            }
        }
    }
}