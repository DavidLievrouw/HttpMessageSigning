using System;

namespace Dalion.HttpMessageSigning.Verification {
    /// <inheritdoc />
    public class HeaderMissingSignatureVerificationFailure : SignatureVerificationFailure {
        /// <inheritdoc />
        public HeaderMissingSignatureVerificationFailure(string message, Exception ex) : base(
            "HEADER_MISSING",
            message,
            ex) {}
    }
}