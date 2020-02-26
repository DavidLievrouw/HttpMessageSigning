using System;
using System.Net.Http;
using System.Security.Cryptography;
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

        public Task Verify(HttpRequestMessage signedRequest, Signature signature, Client client) {
            if (!signature.Created.HasValue) {
                throw new SignatureVerificationException($"The signature does not contain a value for the {nameof(signature.Created)} property, but it is required.");
            }           
            
            if (!signature.Expires.HasValue) {
                throw new SignatureVerificationException($"The signature does not contain a value for the {nameof(signature.Expires)} property, but it is required.");
            }
            
            var signingSettings = new SigningSettings {
                KeyId = client.Id,
                Headers = signature.Headers,
                SignatureAlgorithm = client.SignatureAlgorithm,
                Expires = signature.Expires.Value - signature.Created.Value,
                DigestHashAlgorithm = HashAlgorithmName.SHA256 // ToDo: Does not make sense here. 
            };
            var signingString = _signingStringComposer.Compose(signedRequest, signingSettings, signature.Created.Value);
            var signatureHash = signingSettings.SignatureAlgorithm.ComputeHash(signingString);
            var signatureString = _base64Converter.ToBase64(signatureHash);

            if (signature.String != signatureString) {
                throw new SignatureVerificationException("The signature string verification failed.");
            }
            
            return Task.CompletedTask;
        }
    }
}