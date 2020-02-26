using System;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.SigningString;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    internal class MatchingSignatureVerificationTask : IVerificationTask {
        private readonly ISigningStringComposer _signingStringComposer;
        private readonly IBase64Converter _base64Converter;

        public MatchingSignatureVerificationTask(ISigningStringComposer signingStringComposer, IBase64Converter base64Converter) {
            _signingStringComposer = signingStringComposer ?? throw new ArgumentNullException(nameof(signingStringComposer));
            _base64Converter = base64Converter ?? throw new ArgumentNullException(nameof(base64Converter));
        }

        public Task Verify(HttpRequestForSigning signedRequest, Signature signature, Client client) {
            if (!signature.Created.HasValue) {
                throw new SignatureVerificationException($"The signature does not contain a value for the {nameof(signature.Created)} property, but it is required.");
            }           
            
            if (!signature.Expires.HasValue) {
                throw new SignatureVerificationException($"The signature does not contain a value for the {nameof(signature.Expires)} property, but it is required.");
            }

            var expires = signature.Expires.Value - signature.Created.Value;
            var signingString = _signingStringComposer.Compose(signedRequest, signature.Headers, signature.Created.Value, expires);
            var signatureHash = client.SignatureAlgorithm.ComputeHash(signingString);
            var signatureString = _base64Converter.ToBase64(signatureHash);

            if (signature.String != signatureString) {
                throw new SignatureVerificationException("The signature string verification failed.");
            }
            
            return Task.CompletedTask;
        }
    }
}