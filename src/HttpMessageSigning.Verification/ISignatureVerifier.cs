using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification {
    internal interface ISignatureVerifier {
        Task VerifySignature(Signature signature, Client client);
    }
}