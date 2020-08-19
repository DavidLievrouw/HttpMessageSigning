namespace Dalion.HttpMessageSigning.Verification {
    /// <summary>
    ///     Represents the result of the request signature verification.
    /// </summary>
    public abstract class RequestSignatureVerificationResult {
        /// <summary>
        ///     Creates a new instance of this class.
        /// </summary>
        /// <param name="client">The client for which the verification happened.</param> 
        /// <param name="requestForVerification">The data of the request that was used to verify.</param>
        protected RequestSignatureVerificationResult(Client client, HttpRequestForVerification requestForVerification) {
            Client = client; // Can be null, because a failure might have occurred when looking up the client
            RequestForVerification = requestForVerification; // Can be null, because a failure might have occurred before extracting the data
        }

        /// <summary>
        ///     Gets the client for which the verification happened.
        /// </summary>
        public Client Client { get; }

        /// <summary>
        /// Gets the data of the request that was used to verify.
        /// </summary>
        public HttpRequestForVerification RequestForVerification { get; }

        /// <summary>
        ///     Gets a value indicating whether the signature was successfully verified.
        /// </summary>
        public abstract bool IsSuccess { get; }
    }
}