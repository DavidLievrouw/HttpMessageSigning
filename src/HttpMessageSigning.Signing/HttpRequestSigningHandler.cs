using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Signing {
    public class HttpRequestSigningHandler : DelegatingHandler {
        private readonly IRequestSigner _requestSigner;
            
        public HttpRequestSigningHandler(IRequestSigner requestSigner) {
            _requestSigner = requestSigner ?? throw new ArgumentNullException(nameof(requestSigner));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            await _requestSigner.Sign(request);
            return await base.SendAsync(request, cancellationToken);
        }
    }
}