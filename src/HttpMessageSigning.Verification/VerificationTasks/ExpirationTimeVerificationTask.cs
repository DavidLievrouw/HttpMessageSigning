using System;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    internal class ExpirationTimeVerificationTask : IVerificationTask {
        private readonly ISystemClock _systemClock;
        
        public ExpirationTimeVerificationTask(ISystemClock systemClock) {
            _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
        }

        public Task<SignatureVerificationFailure> Verify(HttpRequestForSigning signedRequest, Signature signature, Client client) {
            if (!signature.Expires.HasValue) {
                return SignatureVerificationFailure.HeaderMissing($"The signature does not contain a value for the {nameof(signature.Expires)} property, but it is required.")
                    .ToTask<SignatureVerificationFailure>();
            }
            
            if (signature.Expires.Value < _systemClock.UtcNow) {
                return SignatureVerificationFailure.SignatureExpired("The signature is expired.")
                    .ToTask<SignatureVerificationFailure>();
            }
            
            return Task.FromResult<SignatureVerificationFailure>(null);
        }
    }
}