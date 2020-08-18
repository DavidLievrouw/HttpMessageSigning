using System;

namespace Dalion.HttpMessageSigning.Verification {
    /// <inheritdoc />
    public class InvalidSignatureSignatureVerificationFailure : SignatureVerificationFailure {
        /// <inheritdoc />
        public InvalidSignatureSignatureVerificationFailure(string message, Exception ex) : base(
            "INVALID_SIGNATURE",
            message,
            ex) {}
    }
}