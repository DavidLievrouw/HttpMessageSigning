using System;

namespace Dalion.HttpMessageSigning.Verification {
    public class HeaderMissingSignatureVerificationFailure : SignatureVerificationFailure {
        public HeaderMissingSignatureVerificationFailure(string message, Exception ex) : base(
            "HEADER_MISSING",
            message,
            ex) {}
    }
}