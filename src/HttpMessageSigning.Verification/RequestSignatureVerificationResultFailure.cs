using System;

namespace Dalion.HttpMessageSigning.Verification {
    /// <summary>
    /// Represents a request signature verification failure.
    /// </summary>
    public class RequestSignatureVerificationResultFailure : RequestSignatureVerificationResult {
        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="client">The client for which the verification happened.</param>
        /// <param name="requestForVerification">The data of the request that was used to verify.</param>
        /// <param name="failure">An object that indicates why the verification failed.</param>
        public RequestSignatureVerificationResultFailure(
            Client client, 
            HttpRequestForVerification requestForVerification,
            SignatureVerificationFailure failure) : base(client, requestForVerification) {
            Failure = failure ?? throw new ArgumentNullException(nameof(failure));
        }

        /// <summary>
        /// Gets an object that indicates why the verification failed.
        /// </summary>
        public SignatureVerificationFailure Failure { get; }
        
        /// <summary>
        /// Gets a value indicating whether the signature was successfully verified.
        /// </summary>
        public override bool IsSuccess => false;
    }
}