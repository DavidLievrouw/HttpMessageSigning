using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning;
using Dalion.HttpMessageSigning.Signing;
using Microsoft.Extensions.DependencyInjection;
using PemUtils;
using Serilog;

namespace Conformance {
    public class Signer {
        private readonly IRequestSignerFactory _requestSignerFactory;

        public Signer() {
            var serviceProvider = new ServiceCollection()
                .AddHttpMessageSigning()
                .BuildServiceProvider();
            _requestSignerFactory = serviceProvider.GetRequiredService<IRequestSignerFactory>();
        }

        public async Task<int> Run(SignOptions options, string httpMessage) {
            RSAParameters privateKey;
            using (var stream = File.OpenRead(options.PrivateKey)) {
                using (var reader = new PemReader(stream))
                {
                    privateKey = reader.ReadRsaKey();
                }
            }
            
            var signer = _requestSignerFactory.Create(
                new KeyId("test"),
                new SigningSettings {
                    SignatureAlgorithm = RSASignatureAlgorithm.CreateForSigning(HashAlgorithmName.SHA256, privateKey),
                    EnableNonce = false,
                    DigestHashAlgorithm = default,
                    AutomaticallyAddRecommendedHeaders = false,
                    Headers = options.Headers
                        ?.Split(new [] {' ', ','}, StringSplitOptions.RemoveEmptyEntries)
                        .Select(h => new HeaderName(h))
                        .ToArray()
                });
            
            var request = HttpRequestMessageParser.Parse(httpMessage);
            await signer.Sign(request);
            
            Log.Information(request.Headers.Authorization.Scheme + " " + request.Headers.Authorization.Parameter);
            
            Console.Out.Flush();
            
            return 1;
        }
    }
}