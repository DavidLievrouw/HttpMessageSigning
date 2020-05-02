using System;
using Dalion.HttpMessageSigning.SigningString;
using Microsoft.Extensions.Logging;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    internal class MatchingSignatureStringVerificationTask : VerificationTask {
        private readonly ISigningStringComposer _signingStringComposer;
        private readonly IBase64Converter _base64Converter;
        private readonly ISystemClock _systemClock;
        private readonly ILogger<MatchingSignatureStringVerificationTask> _logger;

        public MatchingSignatureStringVerificationTask(
            ISigningStringComposer signingStringComposer, 
            IBase64Converter base64Converter,
            ISystemClock systemClock,
            ILogger<MatchingSignatureStringVerificationTask> logger = null) {
            _signingStringComposer = signingStringComposer ?? throw new ArgumentNullException(nameof(signingStringComposer));
            _base64Converter = base64Converter ?? throw new ArgumentNullException(nameof(base64Converter));
            _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
            _logger = logger;
        }

        public override SignatureVerificationFailure VerifySync(HttpRequestForSigning signedRequest, Signature signature, Client client) {
            var expires = signature.Created.HasValue && signature.Expires.HasValue 
                ? signature.Expires.Value - signature.Created.Value
                : new TimeSpan?();

            var signingString = _signingStringComposer.Compose(
                signedRequest, 
                signature.Headers,
                signature.Created ?? _systemClock.UtcNow, 
                expires, 
                signature.Nonce);
            
            _logger?.LogDebug("Composed the following signing string for request verification: {0}", signingString);

            byte[] receivedSignature;
            try {
                receivedSignature = _base64Converter.FromBase64(signature.String);
            }
            catch (FormatException ex) {
                return SignatureVerificationFailure.InvalidSignatureString(ex.Message, ex);
            }
            
            var isValidSignature = client.SignatureAlgorithm.VerifySignature(signingString, receivedSignature);

            _logger?.LogDebug("The verification of the signature {0}.", isValidSignature ? "succeeded" : "failed");
            
            if (!isValidSignature) {
                return SignatureVerificationFailure.InvalidSignatureString("The signature string does not match the expected value.");
            }

            return null;
        }
    }
}