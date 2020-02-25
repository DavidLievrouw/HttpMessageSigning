using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification {
    internal class SignatureVerifier : ISignatureVerifier {
        public Task VerifySignature(Signature signature, Client client) {
            // ToDo: Implement verification here, and throw when invalid
            return Task.CompletedTask;
        }
    }
}