using System;
using System.Net.Http;

namespace Dalion.HttpMessageSigning.Composing {
    internal class RequestTargetHeaderAppender : IHeaderAppender {
        private readonly HttpRequestMessage _request;
        
        public RequestTargetHeaderAppender(HttpRequestMessage request) {
            _request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public string BuildStringToAppend(HeaderName header) {
            if (!_request.RequestUri.IsAbsoluteUri) throw new HttpMessageSigningInvalidRequestException("Cannot sign a request that uses a relative uri.");
            
            return "\n" + new Header(
                       HeaderName.PredefinedHeaderNames.RequestTarget,
                       $"{_request.Method.Method.ToLowerInvariant()} {_request.RequestUri.LocalPath}");
        }
    }
}