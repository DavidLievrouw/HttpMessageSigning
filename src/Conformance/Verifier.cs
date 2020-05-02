using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning;
using Dalion.HttpMessageSigning.Verification;
using Dalion.HttpMessageSigning.Verification.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using PemUtils;

namespace Conformance {
    public class Verifier {
        public async Task<int> Run(VerifyOptions options, string httpMessage) {
            RSAParameters publicKey;
            using (var stream = File.OpenRead(options.PublicKey)) {
                using (var reader = new PemReader(stream)) {
                    publicKey = reader.ReadRsaKey();
                }
            }

            var serviceProvider = new ServiceCollection()
                .AddHttpMessageSignatureVerification(provider => {
                    var clientStore = new InMemoryClientStore();
                    clientStore.Register(new Client(
                        new KeyId("test"),
                        "ConformanceClient",
                        SignatureAlgorithm.CreateForVerification(publicKey),
                        TimeSpan.FromSeconds(30)));
                    return clientStore;
                })
                .BuildServiceProvider();

            var verifier = serviceProvider.GetRequiredService<IRequestSignatureVerifier>();

            var clientRequest = HttpRequestMessageParser.Parse(httpMessage);
            var requestToVerify = await clientRequest.ToServerSideHttpRequest();

            var verificationResult = await verifier.VerifySignature(requestToVerify, new SignedRequestAuthenticationOptions());

            return verificationResult is RequestSignatureVerificationResultSuccess
                ? 0
                : 1;
        }
    }
}