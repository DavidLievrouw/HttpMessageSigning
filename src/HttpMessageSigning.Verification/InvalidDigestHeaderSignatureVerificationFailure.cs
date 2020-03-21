using System;

namespace Dalion.HttpMessageSigning.Verification {
    public class InvalidDigestHeaderSignatureVerificationFailure : SignatureVerificationFailure {
        public InvalidDigestHeaderSignatureVerificationFailure(string message, Exception ex) : base(
            "INVALID_DIGEST_HEADER",
            message,
            ex) {}
    }
}