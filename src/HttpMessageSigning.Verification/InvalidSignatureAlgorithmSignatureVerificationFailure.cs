using System;

namespace Dalion.HttpMessageSigning.Verification {
    /// <inheritdoc />
    public class InvalidSignatureAlgorithmSignatureVerificationFailure : SignatureVerificationFailure {
        /// <inheritdoc />
        public InvalidSignatureAlgorithmSignatureVerificationFailure(string message, Exception ex) : base(
            "INVALID_SIGNATURE_ALGORITHM",
            message,
            ex) {}
    }
}