using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification {
    internal interface ISignatureVerifier {
        Task<SignatureVerificationFailure> VerifySignature(HttpRequestForSigning signedRequest, Signature signature, Client client);
    }
}