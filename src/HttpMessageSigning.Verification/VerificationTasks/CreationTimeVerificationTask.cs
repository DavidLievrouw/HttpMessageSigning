using System;
using System.Linq;
using Dalion.HttpMessageSigning.Utils;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    internal class CreationTimeVerificationTask : VerificationTask {
        private readonly ISystemClock _systemClock;
        
        public CreationTimeVerificationTask(ISystemClock systemClock) {
            _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
        }

        public override SignatureVerificationFailure VerifySync(HttpRequestForVerification signedRequest, Signature signature, Client client) {
            if (signature.Headers.Contains(HeaderName.PredefinedHeaderNames.Created) && !signature.Created.HasValue) {
                return SignatureVerificationFailure.InvalidCreatedHeader($"The signature does not contain a value for the {nameof(signature.Created)} property, but it is required.");
            }
            
            if (signature.Created.HasValue && signature.Created.Value > _systemClock.UtcNow.Add(client.ClockSkew)) {
                return SignatureVerificationFailure.InvalidCreatedHeader("The signature is not valid yet. Its creation time is in the future.");
            }
            
            return null;
        }
    }
}