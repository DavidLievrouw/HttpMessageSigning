using System;

namespace Dalion.HttpMessageSigning.Verification {
    /// <summary>
    /// Represents a request signature verification failure.
    /// </summary>
    public class RequestSignatureVerificationResultFailure : RequestSignatureVerificationResult {
        internal RequestSignatureVerificationResultFailure(SignatureVerificationException signatureVerificationException) {
            SignatureVerificationException = signatureVerificationException ?? throw new ArgumentNullException(nameof(signatureVerificationException));
        }

        /// <summary>
        /// Gets the exception that caused the verification failure.
        /// </summary>
        public SignatureVerificationException SignatureVerificationException { get; }
        
        /// <summary>
        /// Gets a value indicating whether the signature was successfully verified.
        /// </summary>
        public override bool IsSuccess => false;
    }
}