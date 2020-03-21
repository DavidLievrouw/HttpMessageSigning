using System;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    internal class CreationTimeVerificationTask : IVerificationTask {
        private readonly ISystemClock _systemClock;
        
        public CreationTimeVerificationTask(ISystemClock systemClock) {
            _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
        }

        public Task<SignatureVerificationFailure> Verify(HttpRequestForSigning signedRequest, Signature signature, Client client) {
            if (!signature.Created.HasValue) {
                return SignatureVerificationFailure.InvalidCreatedHeader($"The signature does not contain a value for the {nameof(signature.Created)} property, but it is required.")
                    .ToTask<SignatureVerificationFailure>();
            }
            
            if (signature.Created.Value > _systemClock.UtcNow) {
                return SignatureVerificationFailure.InvalidCreatedHeader("The signature is not valid yet. Its creation time is in the future.")
                    .ToTask<SignatureVerificationFailure>();
            }
            
            return Task.FromResult<SignatureVerificationFailure>(null);
        }
    }
}