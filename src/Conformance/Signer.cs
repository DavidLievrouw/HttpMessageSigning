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

            if (options.Algorithm != "hs2019") {
                throw new HttpMessageSigningException("Unsupported algorithm.");
            }

            var signingSettings = new SigningSettings {
                SignatureAlgorithm = RSASignatureAlgorithm.CreateForSigning(HashAlgorithmName.SHA256, privateKey),
                EnableNonce = false,
                DigestHashAlgorithm = default,
                AutomaticallyAddRecommendedHeaders = false,
                Headers = options.Headers
                    ?.Split(new [] {' ', ','}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(h => new HeaderName(h))
                    .ToArray()
            };
            var signer = _requestSignerFactory.Create(
                new KeyId("test"),
                signingSettings);
            
            var request = HttpRequestMessageParser.Parse(httpMessage);
            
            var created = DateTimeOffset.UtcNow;
            if (!string.IsNullOrEmpty(options.Created)) {
                var createdUnix = int.Parse(options.Created);
                created = DateTimeOffset.FromUnixTimeSeconds(createdUnix);
            }
            
            var expires = signingSettings.Expires;
            if (!string.IsNullOrEmpty(options.Expires)) {
                var expiresUnix = int.Parse(options.Expires);
                var expiresAbsolute = DateTimeOffset.FromUnixTimeSeconds(expiresUnix);
                expires = expiresAbsolute - created;
            }
            
            await signer.Sign(request, created, expires);
            
            Log.Information(request.Headers.Authorization.Scheme + " " + request.Headers.Authorization.Parameter);
            
            Console.Out.Flush();
            
            return 0;
        }
    }
}