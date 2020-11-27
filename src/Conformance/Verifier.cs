using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning;
using Dalion.HttpMessageSigning.TestUtils;
using Dalion.HttpMessageSigning.Verification;
using Dalion.HttpMessageSigning.Verification.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using PemUtils;

namespace Conformance {
    public class Verifier {
        public async Task<int> Run(VerifyOptions options, string httpMessage) {
            ISignatureAlgorithm signatureAlgorithmForVerification;
            if (string.IsNullOrEmpty(options.KeyType) || options.KeyType.Equals("RSA", StringComparison.OrdinalIgnoreCase)) {
                RSAParameters rsaPublicKey;
                using (var stream = File.OpenRead(options.PublicKey)) {
                    using (var reader = new PemReader(stream)) {
                        rsaPublicKey = reader.ReadRsaKey();
                    }
                }
                signatureAlgorithmForVerification = RSASignatureAlgorithm.CreateForVerification(HashAlgorithmName.SHA512, rsaPublicKey);
            }
            else if (options.KeyType.Equals("P256", StringComparison.OrdinalIgnoreCase) || options.KeyType.Equals("ECDSA", StringComparison.OrdinalIgnoreCase)) {
                ECParameters ecPublicKey;
                using (var stream = File.OpenRead(options.PublicKey)) {
                    using (var reader = new StreamReader(stream)) {
                        var fileContents = reader.ReadToEnd();
                        var lines = fileContents.Split(new[] {'\n', '\r'}, StringSplitOptions.RemoveEmptyEntries);
                        lines = lines.Skip(1).Take(lines.Length - 2).ToArray();
                        var pem = string.Join("", lines);
                        var ecdsa = ECDsa.Create();
                        var derArray = Convert.FromBase64String(pem);
                        ecdsa.ImportSubjectPublicKeyInfo(derArray, out _);
                        ecPublicKey = ecdsa.ExportParameters(false);
                    }
                }
                signatureAlgorithmForVerification = ECDsaSignatureAlgorithm.CreateForVerification(HashAlgorithmName.SHA512, ecPublicKey);
            } else if (options.KeyType.Equals("HMAC", StringComparison.OrdinalIgnoreCase)) {
                signatureAlgorithmForVerification = SignatureAlgorithm.CreateForVerification(options.PublicKey, HashAlgorithmName.SHA512);
            }
            else {
                throw new NotSupportedException("The specified key type is not supported.");
            }

            var serviceProvider = new ServiceCollection()
                .AddHttpMessageSignatureVerification()
                .UseClient(Client.Create(
                    "test",
                    "ConformanceClient",
                    signatureAlgorithmForVerification
                ))
                .Services
                .BuildServiceProvider();
            
            var verifier = serviceProvider.GetRequiredService<IRequestSignatureVerifier>();

            var clientRequest = HttpRequestMessageParser.Parse(httpMessage);
            var requestToVerify = await clientRequest.ToServerSideHttpRequest();
            
            var verificationResult = await verifier.VerifySignature(requestToVerify, new SignedRequestAuthenticationOptions {
                OnSignatureParsed = (request, signature) => {
                    if (!string.IsNullOrEmpty(options.Algorithm)) {
                        signature.Algorithm = options.Algorithm;
                    }

                    return Task.CompletedTask;
                }
            });
            
            return verificationResult is RequestSignatureVerificationResultSuccess
                ? 0
                : 1;
        }
    }
}