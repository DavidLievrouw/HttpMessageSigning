using System;

namespace Dalion.HttpMessageSigning.Verification {
    /// <inheritdoc />
    public class ReplayedRequestSignatureVerificationFailure : SignatureVerificationFailure {
        /// <inheritdoc />
        public ReplayedRequestSignatureVerificationFailure(string message, Exception ex) : base(
            "REPLAYED_REQUEST",
            message,
            ex) {}
    }
}