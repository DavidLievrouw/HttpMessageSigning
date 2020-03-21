using System;

namespace Dalion.HttpMessageSigning.Verification {
    public class InvalidExpiresHeaderSignatureVerificationFailure : SignatureVerificationFailure {
        public InvalidExpiresHeaderSignatureVerificationFailure(string message, Exception ex) : base(
            "INVALID_EXPIRES_HEADER",
            message,
            ex) {}
    }
}