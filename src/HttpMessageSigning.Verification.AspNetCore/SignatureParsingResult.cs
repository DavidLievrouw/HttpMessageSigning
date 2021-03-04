namespace Dalion.HttpMessageSigning.Verification.AspNetCore {
    /// <summary>
    /// A base class to identify header parsing results.
    /// <seealso cref="SignatureParsingSuccess"/>
    /// <seealso cref="SignatureParsingFailure"/>
    /// </summary>
    public abstract class SignatureParsingResult {
        /// <summary>
        /// True if the parsing result was successful
        /// </summary>
        public abstract bool IsSuccess { get; }
    }
}