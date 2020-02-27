namespace Dalion.HttpMessageSigning.Verification {
    /// <summary>
    /// Represents the result of the request signature verification.
    /// </summary>
    public abstract class RequestSignatureVerificationResult {
        protected RequestSignatureVerificationResult(Client client) {
            Client = client; // Can be null, because a failure might have occurred when looking up the client
        }

        /// <summary>
        /// Gets the client for which the verification happened.
        /// </summary>
        public Client Client { get; }
        
        /// <summary>
        /// Gets a value indicating whether the signature was successfully verified.
        /// </summary>
        public abstract bool IsSuccess { get; }
    }
}