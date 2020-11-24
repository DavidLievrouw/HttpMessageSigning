using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Mime;
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
                .UseSignatureAlgorithm(SignatureAlgorithm.CreateForSigning("yumACY64r%hm"))
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
        public async Task Sign() {
            for (var i = 0; i < 10000; i++) {
                await _requestSigner.Sign(_request);
            }
        }
        
        public async Task SignABunchOfTimes() {
            var watch = Stopwatch.StartNew();
            for (var i = 0; i < 1000000; i++) {
                await _requestSigner.Sign(_request);
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