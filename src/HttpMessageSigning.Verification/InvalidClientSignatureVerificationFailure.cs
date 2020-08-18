using System;

namespace Dalion.HttpMessageSigning.Verification {
    /// <inheritdoc />
    public class InvalidClientSignatureVerificationFailure : SignatureVerificationFailure {
        /// <inheritdoc />
        public InvalidClientSignatureVerificationFailure(string message, Exception ex) : base(
            "INVALID_CLIENT",
            message,
            ex) {}
    }
}