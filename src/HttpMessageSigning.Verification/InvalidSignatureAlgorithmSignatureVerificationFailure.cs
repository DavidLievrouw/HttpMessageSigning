using System;

namespace Dalion.HttpMessageSigning.Verification {
    public class InvalidSignatureAlgorithmSignatureVerificationFailure : SignatureVerificationFailure {
        public InvalidSignatureAlgorithmSignatureVerificationFailure(string message, Exception ex) : base(
            "INVALID_SIGNATURE_ALGORITHM",
            message,
            ex) {}
    }
}