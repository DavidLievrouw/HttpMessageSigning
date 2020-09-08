using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification {
    internal interface ISignatureVerifier {
        Task<SignatureVerificationFailure> VerifySignature(HttpRequestForVerification signedRequest, Client client);
    }
}