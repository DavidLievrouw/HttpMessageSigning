using System;
using System.Linq;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    internal class ExpiresHeaderGuardVerificationTask : VerificationTask {
        private static readonly string[] AlgorithmNamesThatDoNotRequireExpiresValue = {"rsa", "hmac", "ecdsa"};

        public override SignatureVerificationFailure VerifySync(HttpRequestForSigning signedRequest, Signature signature, Client client) {
            if (!AlgorithmNamesThatDoNotRequireExpiresValue.Contains(client.SignatureAlgorithm.Name, StringComparer.OrdinalIgnoreCase) && !signature.Expires.HasValue) {
                return SignatureVerificationFailure.InvalidExpiresHeader($"The signature does not contain a value for the {nameof(signature.Expires)} property, but it is required.");
            }

            return null;
        }
    }
}