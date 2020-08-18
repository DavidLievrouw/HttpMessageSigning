using System;

namespace Dalion.HttpMessageSigning.Verification.AspNetCore {
    internal class SignatureParsingSuccess : SignatureParsingResult {
        public SignatureParsingSuccess(Signature signature) {
            Signature = signature ?? throw new ArgumentNullException(nameof(signature));
        }

        public override bool IsSuccess => true;

        public Signature Signature { get; }
    }
}