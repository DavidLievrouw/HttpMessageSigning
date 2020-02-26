using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    internal interface IVerificationTask {
        Task Verify(HttpRequestForSigning signedRequest, Signature signature, Client client);
    }
}