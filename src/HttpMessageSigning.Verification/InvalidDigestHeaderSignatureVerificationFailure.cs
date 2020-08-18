using System;

namespace Dalion.HttpMessageSigning.Verification {
    /// <inheritdoc />
    public class InvalidDigestHeaderSignatureVerificationFailure : SignatureVerificationFailure {
        /// <inheritdoc />
        public InvalidDigestHeaderSignatureVerificationFailure(string message, Exception ex) : base(
            "INVALID_DIGEST_HEADER",
            message,
            ex) {}
    }
}