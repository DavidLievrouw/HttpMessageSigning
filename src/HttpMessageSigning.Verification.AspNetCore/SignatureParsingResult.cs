namespace Dalion.HttpMessageSigning.Verification.AspNetCore {
    internal abstract class SignatureParsingResult {
        public abstract bool IsSuccess { get; }
    }
}