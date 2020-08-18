using System;

namespace Dalion.HttpMessageSigning.Verification {
    /// <inheritdoc />
    public class SignatureExpiredSignatureVerificationFailure : SignatureVerificationFailure {
        /// <inheritdoc />
        public SignatureExpiredSignatureVerificationFailure(string message, Exception ex) : base(
            "SIGNATURE_EXPIRED",
            message,
            ex) {}
    }
}