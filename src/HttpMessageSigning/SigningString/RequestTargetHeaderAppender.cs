using System;
using Dalion.HttpMessageSigning.SigningString.RequestTarget;

namespace Dalion.HttpMessageSigning.SigningString {
    internal class RequestTargetHeaderAppender : IHeaderAppender {
        private readonly HttpRequestForSignatureString _request;
        private readonly RequestTargetEscaping _requestTargetEscaping;
        private readonly IRequestTargetEscaper _requestTargetEscaper;

        public RequestTargetHeaderAppender(HttpRequestForSignatureString request, RequestTargetEscaping requestTargetEscaping, IRequestTargetEscaper requestTargetEscaper) {
            _request = request ?? throw new ArgumentNullException(nameof(request));
            _requestTargetEscaping = requestTargetEscaping;
            _requestTargetEscaper = requestTargetEscaper ?? throw new ArgumentNullException(nameof(requestTargetEscaper));
        }

        public string BuildStringToAppend(HeaderName header) {
            var path = _requestTargetEscaper.Escape(_request.RequestUri, _requestTargetEscaping);

            return "\n" + new Header(
                       HeaderName.PredefinedHeaderNames.RequestTarget,
                       $"{_request.Method.Method.ToLowerInvariant()} {path}");
        }
    }
}