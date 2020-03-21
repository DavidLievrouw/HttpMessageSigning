using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    internal interface IVerificationTask {
        Task<SignatureVerificationFailure> Verify(HttpRequestForSigning signedRequest, Signature signature, Client client);
    }
}