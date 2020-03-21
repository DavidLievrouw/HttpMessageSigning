using System;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    internal class CreationTimeVerificationTask : VerificationTask {
        private readonly ISystemClock _systemClock;
        
        public CreationTimeVerificationTask(ISystemClock systemClock) {
            _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
        }

        public override SignatureVerificationFailure VerifySync(HttpRequestForSigning signedRequest, Signature signature, Client client) {
            if (!signature.Created.HasValue) {
                return SignatureVerificationFailure.InvalidCreatedHeader($"The signature does not contain a value for the {nameof(signature.Created)} property, but it is required.");
            }
            
            if (signature.Created.Value > _systemClock.UtcNow) {
                return SignatureVerificationFailure.InvalidCreatedHeader("The signature is not valid yet. Its creation time is in the future.");
            }
            
            return null;
        }
    }
}