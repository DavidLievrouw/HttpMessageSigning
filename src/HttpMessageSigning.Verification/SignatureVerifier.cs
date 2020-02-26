using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification {
    internal class SignatureVerifier : ISignatureVerifier {
        public Task VerifySignature(Signature signature, Client client) {
            // ToDo: Implement verification here, and throw when invalid
            
            // SignatureAlgorithm and HashAlgorithm from KeyId should match Algorithm parameter
            // Throw on unsupported algorithm
            // Throw when RSA or HMAC and (created) is specified
            // Header specified in settings, and missing in request: Throw!
            // Created in the future: Throw!
            // Expires in the past: Throw!
            // Signature should match
            
            return Task.CompletedTask;
        }
    }
}