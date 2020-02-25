using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Dalion.HttpMessageSigning.Verification {
    /// <summary>
    /// Service that validates a signature of the specified request, in the form of an Authorization header.
    /// </summary>
    public interface IRequestSignatureValidator {
        /// <summary>
        /// Validate the signature of the specified request.
        /// </summary>
        /// <param name="request">The request to validate the signature for.</param>
        /// <returns>A validation result that indicates success or failure.</returns>
        Task<RequestSignatureValidationResult> ValidateSignature(HttpRequest request);
    }
}