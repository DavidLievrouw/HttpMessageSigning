using System.Net.Http;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    public class AllHeadersPresentVerificationTask : IVerificationTask {
        public Task Verify(HttpRequestMessage signedRequest, Signature signature, Client client) {
            throw new System.NotImplementedException();
        }
    }
}