namespace Dalion.HttpMessageSigning.Verification {
    /// <summary>
    /// Represents the result of the request signature verification.
    /// </summary>
    public abstract class RequestSignatureVerificationResult {
        /// <summary>
        /// Gets a value indicating whether the signature was successfully verified.
        /// </summary>
        public abstract bool IsSuccess { get; }
    }
}