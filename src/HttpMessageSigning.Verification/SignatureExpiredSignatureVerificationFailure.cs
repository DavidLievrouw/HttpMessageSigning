using System;

namespace Dalion.HttpMessageSigning.Verification {
    public class SignatureExpiredSignatureVerificationFailure : SignatureVerificationFailure {
        public SignatureExpiredSignatureVerificationFailure(string message, Exception ex) : base(
            "SIGNATURE_EXPIRED",
            message,
            ex) {}
    }
}