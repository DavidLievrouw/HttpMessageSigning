using System;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification {
    /// <summary>
    ///     Represents a service that orchestrates tasks for verifying a request signature.
    /// </summary>
    public interface IRequestSignatureVerificationOrchestrator : IDisposable {
        /// <summary>
        ///     Verify the signature of the specified request.
        /// </summary>
        /// <param name="request">The request and its signature to verify.</param>
        /// <returns>The result of the verification, success or failure.</returns>
        Task<RequestSignatureVerificationResult> VerifySignature(HttpRequestForVerification request);
    }
}