using System;

namespace Dalion.HttpMessageSigning.Verification {
    public class ReplayedRequestSignatureVerificationFailure : SignatureVerificationFailure {
        public ReplayedRequestSignatureVerificationFailure(string message, Exception ex) : base(
            "REPLAYED_REQUEST",
            message,
            ex) {}
    }
}