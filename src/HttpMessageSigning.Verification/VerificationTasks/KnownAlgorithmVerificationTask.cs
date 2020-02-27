using System;
using System.Linq;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    internal class KnownAlgorithmVerificationTask : IVerificationTask {
        private static readonly string[] SupportedSignatureAlgorithmNames = {"rsa", "hmac"};

        public Task<Exception> Verify(HttpRequestForSigning signedRequest, Signature signature, Client client) {
            // Algorithm parameter is not required
            if (string.IsNullOrEmpty(signature.Algorithm)) return Task.FromResult<Exception>(null);

            var parts = signature.Algorithm.Split('-');
            if (parts.Length < 2) {
                return new SignatureVerificationException($"The specified signature algorithm ({signature.Algorithm}) is not supported.")
                    .ToTask<Exception>();
            }

            if (!SupportedSignatureAlgorithmNames.Contains(parts[0])) {
                return new SignatureVerificationException($"The specified signature algorithm ({signature.Algorithm}) is not supported.")
                    .ToTask<Exception>();
            }

            var hashAlgorithm = System.Security.Cryptography.HashAlgorithm.Create(parts[1].ToUpperInvariant());
            if (hashAlgorithm == null) {
                return new SignatureVerificationException($"The specified hash algorithm ({parts[1]}) is not supported.")
                    .ToTask<Exception>();
            }
            
            return Task.FromResult<Exception>(null);
        }
    }
}