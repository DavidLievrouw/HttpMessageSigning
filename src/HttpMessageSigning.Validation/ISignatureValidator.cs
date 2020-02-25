using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Validation {
    internal interface ISignatureValidator {
        Task ValidateSignature(Signature signature, Client client);
    }
}