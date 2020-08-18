using System;

namespace Dalion.HttpMessageSigning.Verification {
    /// <inheritdoc />
    public class InvalidExpiresHeaderSignatureVerificationFailure : SignatureVerificationFailure {
        /// <inheritdoc />
        public InvalidExpiresHeaderSignatureVerificationFailure(string message, Exception ex) : base(
            "INVALID_EXPIRES_HEADER",
            message,
            ex) {}
    }
}