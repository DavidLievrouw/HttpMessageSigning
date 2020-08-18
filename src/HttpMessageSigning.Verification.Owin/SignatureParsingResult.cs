namespace Dalion.HttpMessageSigning.Verification.Owin {
    internal abstract class SignatureParsingResult {
        public abstract bool IsSuccess { get; }
    }
}