using System;
using System.Collections.Generic;
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
            ISignatureAlgorithm signatureAlgorithm;
            if (string.IsNullOrEmpty(options.KeyType) || options.KeyType.Equals("RSA", StringComparison.OrdinalIgnoreCase)) {
                RSAParameters rsaPrivateKey;
                using (var stream = File.OpenRead(options.PrivateKey)) {
                    using (var reader = new PemReader(stream)) {
                        rsaPrivateKey = reader.ReadRsaKey();
                    }
                }

                signatureAlgorithm = RSASignatureAlgorithm.CreateForSigning(HashAlgorithmName.SHA256, rsaPrivateKey);
            }
            else if (options.KeyType.Equals("P256", StringComparison.OrdinalIgnoreCase) || options.KeyType.Equals("ECDSA", StringComparison.OrdinalIgnoreCase)) {
                ECParameters ecPrivateKey;
                using (var stream = File.OpenRead(options.PrivateKey)) {
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

                signatureAlgorithm = ECDsaSignatureAlgorithm.CreateForSigning(HashAlgorithmName.SHA256, ecPrivateKey);
            }
            else if (options.KeyType.Equals("HMAC", StringComparison.OrdinalIgnoreCase)) {
                signatureAlgorithm = SignatureAlgorithm.CreateForSigning(options.PrivateKey, HashAlgorithmName.SHA256);
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
                DigestHashAlgorithm = default,
                AutomaticallyAddRecommendedHeaders = false,
                Headers = options.Headers
                    ?.Split(new[] {' ', ','}, StringSplitOptions.RemoveEmptyEntries)
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

            var httpMessageLines = new List<string>(httpMessage.Split('\n').Select(l => l.Trim()));
            httpMessageLines.Insert(1, "Authorization: " + request.Headers.Authorization.Scheme + " " + request.Headers.Authorization.Parameter);
            var fullMessage = string.Join('\n', httpMessageLines);
            Log.Information(fullMessage);

            Console.Out.Flush();

            return 0;
        }
    }
}