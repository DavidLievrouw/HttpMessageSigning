using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.HttpMessages;
using Dalion.HttpMessageSigning.Keys;
using Dalion.HttpMessageSigning.Signing;
using Microsoft.Extensions.DependencyInjection;

namespace Dalion.HttpMessageSigning.Sign {
    public static class Signer {
        public static async Task<string> Run(SignOptions options) {
            var serviceProvider = new ServiceCollection()
                .AddHttpMessageSigning()
                .BuildServiceProvider();
            var requestSignerFactory = serviceProvider.GetRequiredService<IRequestSignerFactory>();
            
            ISignatureAlgorithm signatureAlgorithm;
            if (string.IsNullOrEmpty(options.KeyType) || options.KeyType.Equals("RSA", StringComparison.OrdinalIgnoreCase)) {
                signatureAlgorithm = RSASignatureAlgorithm.CreateForSigning(HashAlgorithmName.SHA512, KeyReader.ReadRSA(options.PrivateKey));
            }
            else if (options.KeyType.Equals("P256", StringComparison.OrdinalIgnoreCase) || options.KeyType.Equals("ECDSA", StringComparison.OrdinalIgnoreCase)) {
                signatureAlgorithm = ECDsaSignatureAlgorithm.CreateForSigning(HashAlgorithmName.SHA512, KeyReader.ReadECDsaPrivate(options.PrivateKey));
            }
            else if (options.KeyType.Equals("HMAC", StringComparison.OrdinalIgnoreCase)) {
                signatureAlgorithm = SignatureAlgorithm.CreateForSigning(options.PrivateKey, HashAlgorithmName.SHA512);
            }
            else {
                throw new NotSupportedException("The specified key type is not supported.");
            }

            if (!string.IsNullOrEmpty(options.Algorithm) &&
                !options.Algorithm.StartsWith("rsa", StringComparison.OrdinalIgnoreCase) &&
                !options.Algorithm.StartsWith("hmac", StringComparison.OrdinalIgnoreCase) &&
                !options.Algorithm.StartsWith("ecdsa", StringComparison.OrdinalIgnoreCase)) {
                signatureAlgorithm = new CustomSignatureAlgorithm(options.Algorithm);
            }

            var signingSettings = new SigningSettings {
                SignatureAlgorithm = signatureAlgorithm,
                EnableNonce = false,
                DigestHashAlgorithm = HashAlgorithmName.SHA256,
                AutomaticallyAddRecommendedHeaders = false,
                Headers = options.Headers
                    ?.Split(new[] {' ', ','}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(h => new HeaderName(h))
                    .ToArray()
            };
            var signer = requestSignerFactory.Create(
                new KeyId("test"),
                signingSettings);

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

            await signer.Sign(options.Message, created, expires);

            var serializedMessage = HttpMessageSerializer.Serialize(options.Message);
            
            return serializedMessage;
        }
    }
}