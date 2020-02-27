using System;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    internal class DigestVerificationTask : IVerificationTask {
        public Task<Exception> Verify(HttpRequestForSigning signedRequest, Signature signature, Client client) {
            throw new System.NotImplementedException();
        }
    }
}