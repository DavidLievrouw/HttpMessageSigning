using System;

namespace Dalion.HttpMessageSigning.Validation {
    /// <summary>
    /// Represents a request signature validation failure.
    /// </summary>
    public class RequestSignatureValidationResultFailure : RequestSignatureValidationResult {
        internal RequestSignatureValidationResultFailure(HttpMessageSigningSignatureValidationException signatureValidationException) {
            SignatureValidationException = signatureValidationException ?? throw new ArgumentNullException(nameof(signatureValidationException));
        }

        /// <summary>
        /// Gets the exception that caused the validation failure.
        /// </summary>
        public HttpMessageSigningSignatureValidationException SignatureValidationException { get; }
        
        /// <summary>
        /// Gets a value indicating whether the signature was successfully validated.
        /// </summary>
        public override bool IsSuccess => false;
    }
}