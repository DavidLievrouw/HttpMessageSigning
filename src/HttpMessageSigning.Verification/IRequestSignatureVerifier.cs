using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Dalion.HttpMessageSigning.Verification {
    /// <summary>
    /// Service that verifies a signature of the specified request, in the form of an Authorization header.
    /// </summary>
    public interface IRequestSignatureVerifier : IDisposable {
        /// <summary>
        /// Verify the signature of the specified request.
        /// </summary>
        /// <param name="request">The request to verify the signature for.</param>
        /// <returns>A verification result that indicates success or failure.</returns>
        Task<RequestSignatureVerificationResult> VerifySignature(HttpRequest request);
    }
}