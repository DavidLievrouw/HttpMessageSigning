using System;

namespace Dalion.HttpMessageSigning.Verification {
    public class InvalidSignatureStringSignatureVerificationFailure : SignatureVerificationFailure {
        public InvalidSignatureStringSignatureVerificationFailure(string message, Exception ex) : base(
            "INVALID_SIGNATURE_STRING",
            message,
            ex) {}
    }
}