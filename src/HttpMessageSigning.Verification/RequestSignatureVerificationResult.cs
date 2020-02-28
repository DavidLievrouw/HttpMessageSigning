namespace Dalion.HttpMessageSigning.Verification {
    /// <summary>
    /// Represents the result of the request signature verification.
    /// </summary>
    public abstract class RequestSignatureVerificationResult {
        protected RequestSignatureVerificationResult(Client client, Signature signature) {
            Client = client; // Can be null, because a failure might have occurred when looking up the client
            Signature = signature;
        }

        /// <summary>
        /// Gets the client for which the verification happened.
        /// </summary>
        public Client Client { get; }

        /// <summary>
        /// Gets the signature for which verification was attempted.
        /// </summary>
        public Signature Signature { get; }

        /// <summary>
        /// Gets a value indicating whether the signature was successfully verified.
        /// </summary>
        public abstract bool IsSuccess { get; }
    }
}