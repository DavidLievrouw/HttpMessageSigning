using System;

namespace Dalion.HttpMessageSigning.Verification.AspNetCore {
    internal class SignatureParsingFailure : SignatureParsingResult {
        public SignatureParsingFailure(string description, Exception failure = null) {
            if (string.IsNullOrEmpty(description)) throw new ArgumentException("Value cannot be null or empty.", nameof(description));
            
            Description = description;
            Failure = failure;
        }
        
        public override bool IsSuccess => false;

        public string Description { get; }
        public Exception Failure { get; }
    }
}