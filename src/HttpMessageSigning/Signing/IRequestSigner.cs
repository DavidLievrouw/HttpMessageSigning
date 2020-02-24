using System.Net.Http;

namespace Dalion.HttpMessageSigning.Signing {
    /// <summary>
    /// Service that adds a signature to the specified request, in the form of an Authorization header.
    /// </summary>
    public interface IRequestSigner {
        /// <summary>
        /// Sign the specified request.
        /// </summary>
        /// <param name="request">The request to sign.</param>
        void Sign(HttpRequestMessage request);
    }
}