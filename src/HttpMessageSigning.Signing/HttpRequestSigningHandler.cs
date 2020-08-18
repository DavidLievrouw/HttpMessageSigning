using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Signing {
    /// <summary>
    ///     A HTTP message handler that signs HTTP requests before sending them.
    /// </summary>
    public class HttpRequestSigningHandler : DelegatingHandler {
        private readonly IRequestSigner _requestSigner;

        /// <summary>
        ///     Create a new instance of this class.
        /// </summary>
        /// <param name="requestSigner">The <see cref="IRequestSigner" /> that will sign the request.</param>
        public HttpRequestSigningHandler(IRequestSigner requestSigner) {
            _requestSigner = requestSigner ?? throw new ArgumentNullException(nameof(requestSigner));
        }

        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            await _requestSigner.Sign(request).ConfigureAwait(false);
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}