using System.Net.Http;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    internal interface IVerificationTask {
        Task Verify(HttpRequestMessage signedRequest, Signature signature, Client client);
    }
}