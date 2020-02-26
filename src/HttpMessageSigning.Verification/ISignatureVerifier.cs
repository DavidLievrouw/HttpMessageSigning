using System.Net.Http;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification {
    internal interface ISignatureVerifier {
        Task VerifySignature(HttpRequestMessage signedRequest, Signature signature, Client client);
    }
}