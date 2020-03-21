using System;

namespace Dalion.HttpMessageSigning.Verification {
    public class InvalidClientSignatureVerificationFailure : SignatureVerificationFailure {
        public InvalidClientSignatureVerificationFailure(string message, Exception ex) : base(
            "INVALID_CLIENT",
            message,
            ex) {}
    }
}