using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.Keys;
using Dalion.HttpMessageSigning.Verification;
using Dalion.HttpMessageSigning.Verification.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace Dalion.HttpMessageSigning.Verify {
    public static class Verifier {
        public static async Task<bool> Run(VerifyOptions options) {
            ISignatureAlgorithm signatureAlgorithm;
            if (string.IsNullOrEmpty(options.KeyType) || options.KeyType.Equals("RSA", StringComparison.OrdinalIgnoreCase)) {
                signatureAlgorithm = RSASignatureAlgorithm.CreateForVerification(HashAlgorithmName.SHA512, KeyReader.ReadRSA(options.PublicKey));
            }
            else if (options.KeyType.Equals("P256", StringComparison.OrdinalIgnoreCase) || options.KeyType.Equals("ECDSA", StringComparison.OrdinalIgnoreCase)) {
                signatureAlgorithm = ECDsaSignatureAlgorithm.CreateForVerification(HashAlgorithmName.SHA512, KeyReader.ReadECDsaPublic(options.PublicKey));
            }
            else if (options.KeyType.Equals("HMAC", StringComparison.OrdinalIgnoreCase)) {
                signatureAlgorithm = SignatureAlgorithm.CreateForVerification(options.PublicKey, HashAlgorithmName.SHA512);
            }
            else {
                throw new NotSupportedException("The specified key type is not supported.");
            }

            var serviceProvider = new ServiceCollection()
                .AddHttpMessageSignatureVerification(provider => {
                    var clientStore = new InMemoryClientStore();
                    clientStore.Register(new Client(
                        new KeyId("test"),
                        "ConformanceClient",
                        signatureAlgorithm,
                        TimeSpan.FromSeconds(30),
                        TimeSpan.FromMinutes(1)));
                    return clientStore;
                })
                .BuildServiceProvider();

            var verifier = serviceProvider.GetRequiredService<IRequestSignatureVerifier>();

            var verificationResult = await verifier.VerifySignature(options.Message, new SignedRequestAuthenticationOptions {
                OnSignatureParsed = options.ModifyParsedSignature
            });

            return verificationResult is RequestSignatureVerificationResultSuccess;
        }
    }
}