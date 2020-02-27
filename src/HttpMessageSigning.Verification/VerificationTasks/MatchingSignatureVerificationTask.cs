using System;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.Logging;
using Dalion.HttpMessageSigning.SigningString;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    internal class MatchingSignatureVerificationTask : IVerificationTask {
        private readonly ISigningStringComposer _signingStringComposer;
        private readonly IBase64Converter _base64Converter;
        private readonly IHttpMessageSigningLogger<MatchingSignatureVerificationTask> _logger;

        public MatchingSignatureVerificationTask(
            ISigningStringComposer signingStringComposer, 
            IBase64Converter base64Converter,
            IHttpMessageSigningLogger<MatchingSignatureVerificationTask> logger) {
            _signingStringComposer = signingStringComposer ?? throw new ArgumentNullException(nameof(signingStringComposer));
            _base64Converter = base64Converter ?? throw new ArgumentNullException(nameof(base64Converter));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<Exception> Verify(HttpRequestForSigning signedRequest, Signature signature, Client client) {
            if (!signature.Created.HasValue) {
                return new SignatureVerificationException($"The signature does not contain a value for the {nameof(signature.Created)} property, but it is required.")
                    .ToTask<Exception>();
            }           
            
            if (!signature.Expires.HasValue) {
                return new SignatureVerificationException($"The signature does not contain a value for the {nameof(signature.Expires)} property, but it is required.")
                    .ToTask<Exception>();
            }

            var expires = signature.Expires.Value - signature.Created.Value;
            var signingString = _signingStringComposer.Compose(signedRequest, signature.Headers, signature.Created.Value, expires);
            
            _logger.Debug("Composed the following signing string for request verification: {0}", signingString);
            
            var signatureHash = client.SignatureAlgorithm.ComputeHash(signingString);
            var signatureString = _base64Converter.ToBase64(signatureHash);

            _logger.Debug("The base64 hash of the signature string for verification is '{0}'.", signatureString);
            
            if (signature.String != signatureString) {
                return new SignatureVerificationException("The signature string verification failed.")
                    .ToTask<Exception>();
            }
            
            return Task.FromResult<Exception>(null);
        }
    }
}