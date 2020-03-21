using System;

namespace Dalion.HttpMessageSigning.Verification {
    public class InvalidSignatureSignatureVerificationFailure : SignatureVerificationFailure {
        public InvalidSignatureSignatureVerificationFailure(string message, Exception ex) : base(
            "INVALID_SIGNATURE",
            message,
            ex) {}
    }
}