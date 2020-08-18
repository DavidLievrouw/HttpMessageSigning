using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification {
    public interface ISignatureVerifier {
        Task<SignatureVerificationFailure> VerifySignature(HttpRequestForVerification signedRequest, Client client);
    }
}