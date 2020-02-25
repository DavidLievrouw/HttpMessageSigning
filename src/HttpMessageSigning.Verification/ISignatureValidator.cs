using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification {
    internal interface ISignatureValidator {
        Task ValidateSignature(Signature signature, Client client);
    }
}