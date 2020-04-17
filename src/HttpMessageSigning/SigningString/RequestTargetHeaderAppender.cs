using System;

namespace Dalion.HttpMessageSigning.SigningString {
    internal class RequestTargetHeaderAppender : IHeaderAppender {
        private readonly HttpRequestForSigning _request;
        
        public RequestTargetHeaderAppender(HttpRequestForSigning request) {
            _request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public string BuildStringToAppend(HeaderName header) {
            var path = _request.RequestUri;
                
            return "\n" + new Header(
                       HeaderName.PredefinedHeaderNames.RequestTarget,
                       $"{_request.Method.Method.ToLower()} {path}");
        }
    }
}