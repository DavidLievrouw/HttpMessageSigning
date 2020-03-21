using System;

namespace Dalion.HttpMessageSigning.Verification {
    public class InvalidNonceSignatureVerificationFailure : SignatureVerificationFailure {
        public InvalidNonceSignatureVerificationFailure(string message, Exception ex) : base(
            "INVALID_NONCE",
            message,
            ex) {}
    }
}