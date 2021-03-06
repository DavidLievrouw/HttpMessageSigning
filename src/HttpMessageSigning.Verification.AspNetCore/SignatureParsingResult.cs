namespace Dalion.HttpMessageSigning.Verification.AspNetCore {
    /// <summary>
    ///     A base class for signature parsing results.
    ///     <seealso cref="SignatureParsingSuccess" />
    ///     <seealso cref="SignatureParsingFailure" />
    /// </summary>
    public abstract class SignatureParsingResult {
        /// <summary>
        ///     Gets a value indicating whether or not the parsing was successful.
        /// </summary>
        public abstract bool IsSuccess { get; }
    }
}