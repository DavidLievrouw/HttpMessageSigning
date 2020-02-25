namespace Dalion.HttpMessageSigning.Verification {
    /// <summary>
    /// Represents the result of the request signature validation.
    /// </summary>
    public abstract class RequestSignatureValidationResult {
        /// <summary>
        /// Gets a value indicating whether the signature was successfully validated.
        /// </summary>
        public abstract bool IsSuccess { get; }
    }
}