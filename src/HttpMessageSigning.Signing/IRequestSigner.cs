using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Signing {
    /// <summary>
    /// Service that adds a signature to the specified request, in the form of an Authorization header.
    /// </summary>
    public interface IRequestSigner : IDisposable {
        /// <summary>
        /// Sign the specified request.
        /// </summary>
        /// <param name="request">The request to sign.</param>
        Task Sign(HttpRequestMessage request);
        
        /// <summary>
        /// Sign the specified request.
        /// </summary>
        /// <param name="request">The request to sign.</param>
        /// <param name="timeOfSigning">The time when the signature becomes valid.</param>
        /// <param name="expires">The timespan after which the signature is considered expired.</param>
        Task Sign(HttpRequestMessage request, DateTimeOffset timeOfSigning, TimeSpan expires);
    }
}