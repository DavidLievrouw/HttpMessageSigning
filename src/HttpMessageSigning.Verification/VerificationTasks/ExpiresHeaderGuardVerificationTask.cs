using System;
using System.Linq;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    internal class ExpiresHeaderGuardVerificationTask : VerificationTask {
        private static readonly string[] AlgorithmNamesThatDoNotAllowExpiresValue = {"rsa", "hmac", "ecdsa"};

        public override SignatureVerificationFailure VerifySync(HttpRequestForVerification signedRequest, Signature signature, Client client) {
            if (
                signature.Headers.Contains(HeaderName.PredefinedHeaderNames.Expires) &&
                AlgorithmNamesThatDoNotAllowExpiresValue.Any(alg => signature.Algorithm.StartsWith(alg, StringComparison.OrdinalIgnoreCase))) {
                return SignatureVerificationFailure.InvalidExpiresHeader(
                    $"It is not allowed to take the {HeaderName.PredefinedHeaderNames.Expires} into account, when the signature algorithm is {signature.Algorithm}.");
            }

            return null;
        }
    }
}