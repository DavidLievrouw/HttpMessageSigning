using System;

namespace Dalion.HttpMessageSigning.Verification {
    /// <inheritdoc />
    public class InvalidCreatedHeaderSignatureVerificationFailure : SignatureVerificationFailure {
        /// <inheritdoc />
        public InvalidCreatedHeaderSignatureVerificationFailure(string message, Exception ex) : base(
            "INVALID_CREATED_HEADER",
            message,
            ex) {}
    }
}