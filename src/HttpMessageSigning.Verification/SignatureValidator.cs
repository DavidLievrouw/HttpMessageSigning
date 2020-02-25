using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification {
    internal class SignatureValidator : ISignatureValidator {
        public Task ValidateSignature(Signature signature, Client client) {
            // ToDo: Implement validation here, and throw when invalid
            return Task.CompletedTask;
        }
    }
}