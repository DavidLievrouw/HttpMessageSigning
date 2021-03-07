using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace Dalion.HttpMessageSigning.Verification.AspNetCore {
    /// <summary>
    ///     Extracts the values of the header that contains the signature.
    /// </summary>
    public interface IAuthenticationHeaderExtractor {
        /// <summary>
        ///     Extract the values of the header that contains the signature.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest" /> to extract the values from.</param>
        /// <returns>A <see cref="AuthenticationHeaderValue" /> that contains the Scheme and Parameter, that the specified <see cref="HttpRequest" /> contains.</returns>
        /// <remarks>This method returns <see langword="null" /> when the request does not contain authentication information.</remarks>
        AuthenticationHeaderValue Extract(HttpRequest request);
    }
}