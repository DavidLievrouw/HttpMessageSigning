using System;
using System.Linq;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    internal class CreatedHeaderGuardVerificationTask : VerificationTask {
        private static readonly string[] AlgorithmNamesThatDoNotAllowCreatedValue = {"rsa", "hmac", "ecdsa"};

        public override SignatureVerificationFailure VerifySync(HttpRequestForSigning signedRequest, Signature signature, Client client) {
            if (
                signature.Headers.Contains(HeaderName.PredefinedHeaderNames.Created) &&
                AlgorithmNamesThatDoNotAllowCreatedValue.Any(alg => signature.Algorithm.StartsWith(alg, StringComparison.OrdinalIgnoreCase))) {
                return SignatureVerificationFailure.InvalidCreatedHeader(
                    $"It is not allowed to take the {HeaderName.PredefinedHeaderNames.Created} into account, when the signature algorithm is {signature.Algorithm}.");
            }
            
            if (!AlgorithmNamesThatDoNotAllowCreatedValue.Contains(client.SignatureAlgorithm.Name, StringComparer.OrdinalIgnoreCase) && !signature.Created.HasValue) {
                return SignatureVerificationFailure.InvalidCreatedHeader($"The signature does not contain a value for the {nameof(signature.Created)} property, but it is required.");
            }
            
            return null;
        }
    }
}