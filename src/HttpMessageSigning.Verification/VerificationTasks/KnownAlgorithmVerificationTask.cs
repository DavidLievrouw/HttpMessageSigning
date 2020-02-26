using System.Linq;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    internal class KnownAlgorithmVerificationTask : IVerificationTask {
        private static readonly string[] SupportedSignatureAlgorithmNames = {"rsa", "hmac"};

        public Task Verify(HttpRequestForSigning signedRequest, Signature signature, Client client) {
            // Algorithm parameter is not required
            if (string.IsNullOrEmpty(signature.Algorithm)) return Task.CompletedTask;

            var parts = signature.Algorithm.Split('-');
            if (parts.Length < 2) {
                throw new SignatureVerificationException($"The specified signature algorithm ({signature.Algorithm}) is not supported.");
            }

            if (!SupportedSignatureAlgorithmNames.Contains(parts[0])) {
                throw new SignatureVerificationException($"The specified signature algorithm ({signature.Algorithm}) is not supported.");
            }

            var hashAlgorithm = System.Security.Cryptography.HashAlgorithm.Create(parts[1].ToUpperInvariant());
            if (hashAlgorithm == null) {
                throw new SignatureVerificationException($"The specified hash algorithm ({parts[1]}) is not supported.");
            }
            
            return Task.CompletedTask;
        }
    }
}