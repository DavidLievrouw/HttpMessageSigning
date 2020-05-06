using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.HttpMessages;
using Dalion.HttpMessageSigning.Keys;
using Dalion.HttpMessageSigning.Signing;
using Microsoft.Extensions.DependencyInjection;
using PemUtils;

namespace Dalion.HttpMessageSigning.Sign {
    public static class Signer {
        public static async Task<string> Run(SignOptions options) {
            var serviceProvider = new ServiceCollection()
                .AddHttpMessageSigning()
                .BuildServiceProvider();
            var requestSignerFactory = serviceProvider.GetRequiredService<IRequestSignerFactory>();
            
            ISignatureAlgorithm signatureAlgorithm;
            if (string.IsNullOrEmpty(options.KeyType) || options.KeyType.Equals("RSA", StringComparison.OrdinalIgnoreCase)) {
                RSAParameters rsaPrivateKey;
                
                using (var stream = KeyReader.Read(options.PrivateKey)) {
                    using (var reader = new PemReader(stream)) {
                        rsaPrivateKey = reader.ReadRsaKey();
                    }
                }

                signatureAlgorithm = RSASignatureAlgorithm.CreateForSigning(HashAlgorithmName.SHA512, rsaPrivateKey);
            }
            else if (options.KeyType.Equals("P256", StringComparison.OrdinalIgnoreCase) || options.KeyType.Equals("ECDSA", StringComparison.OrdinalIgnoreCase)) {
                ECParameters ecPrivateKey;
                using (var stream = KeyReader.Read(options.PrivateKey)) {
                    using (var reader = new StreamReader(stream)) {
                        var fileContents = reader.ReadToEnd();
                        var lines = fileContents.Split(new[] {'\n', '\r'}, StringSplitOptions.RemoveEmptyEntries);
                        lines = lines.Skip(1).Take(lines.Length - 2).ToArray();
                        var pem = string.Join("", lines);
                        var ecdsa = ECDsa.Create();
                        var derArray = Convert.FromBase64String(pem);
                        ecdsa.ImportPkcs8PrivateKey(derArray, out _);
                        ecPrivateKey = ecdsa.ExportParameters(true);
                    }
                }

                signatureAlgorithm = ECDsaSignatureAlgorithm.CreateForSigning(HashAlgorithmName.SHA512, ecPrivateKey);
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