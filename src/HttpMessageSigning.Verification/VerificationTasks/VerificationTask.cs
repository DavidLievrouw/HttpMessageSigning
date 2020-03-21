using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    internal abstract class VerificationTask : IVerificationTask {
        public virtual Task<SignatureVerificationFailure> Verify(HttpRequestForSigning signedRequest, Signature signature, Client client) {
            return VerifySync(signedRequest, signature, client).ToTask();
        }
        
        public virtual SignatureVerificationFailure VerifySync(HttpRequestForSigning signedRequest, Signature signature, Client client) {
            throw new System.NotSupportedException();
        }
    }
}