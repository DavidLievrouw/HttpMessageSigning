using System;
using System.Linq;
using Microsoft.Extensions.Primitives;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    internal class CreatedHeaderGuardVerificationTask : VerificationTask {
        private static readonly string[] AlgorithmNamesThatDoNotRequireCreatedValue = {"rsa", "hmac", "ecdsa"};

        public override SignatureVerificationFailure VerifySync(HttpRequestForSigning signedRequest, Signature signature, Client client) {
            if (!AlgorithmNamesThatDoNotRequireCreatedValue.Contains(client.SignatureAlgorithm.Name, StringComparer.OrdinalIgnoreCase) && !signature.Created.HasValue) {
                return SignatureVerificationFailure.InvalidCreatedHeader($"The signature does not contain a value for the {nameof(signature.Created)} property, but it is required.");
            }
            
            return null;
        }
    }
}