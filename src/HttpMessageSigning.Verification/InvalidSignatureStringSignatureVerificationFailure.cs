using System;

namespace Dalion.HttpMessageSigning.Verification {
    /// <inheritdoc />
    public class InvalidSignatureStringSignatureVerificationFailure : SignatureVerificationFailure {
        /// <inheritdoc />
        public InvalidSignatureStringSignatureVerificationFailure(string message, Exception ex) : base(
            "INVALID_SIGNATURE_STRING",
            message,
            ex) {}
    }
}