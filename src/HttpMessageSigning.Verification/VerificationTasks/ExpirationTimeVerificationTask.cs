using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    internal class ExpirationTimeVerificationTask : IVerificationTask {
        private readonly ISystemClock _systemClock;
        
        public ExpirationTimeVerificationTask(ISystemClock systemClock) {
            _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
        }

        public Task Verify(HttpRequestMessage signedRequest, Signature signature, Client client) {
            if (!signature.Expires.HasValue) {
                throw new SignatureVerificationException($"The signature does not contain a value for the {nameof(signature.Expires)} property, but it is required.");
            }
            
            if (signature.Expires.Value < _systemClock.UtcNow) {
                throw new SignatureVerificationException("The signature is expired.");
            }
            
            return Task.CompletedTask;
        }
    }
}