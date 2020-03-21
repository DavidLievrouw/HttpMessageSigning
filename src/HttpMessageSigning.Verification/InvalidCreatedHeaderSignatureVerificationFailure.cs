using System;

namespace Dalion.HttpMessageSigning.Verification {
    public class InvalidCreatedHeaderSignatureVerificationFailure : SignatureVerificationFailure {
        public InvalidCreatedHeaderSignatureVerificationFailure(string message, Exception ex) : base(
            "INVALID_CREATED_HEADER",
            message,
            ex) {}
    }
}