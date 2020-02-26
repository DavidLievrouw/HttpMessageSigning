using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    internal class CreationTimeVerificationTask : IVerificationTask {
        private readonly ISystemClock _systemClock;
        
        public CreationTimeVerificationTask(ISystemClock systemClock) {
            _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
        }

        public Task Verify(HttpRequestMessage signedRequest, Signature signature, Client client) {
            if (!signature.Created.HasValue) {
                throw new SignatureVerificationException($"The signature does not contain a value for the {nameof(signature.Created)} property, but it is required.");
            }
            
            if (signature.Created.Value > _systemClock.UtcNow) {
                throw new SignatureVerificationException("The signature is not valid yet.");
            }
            
            return Task.CompletedTask;
        }
    }
}