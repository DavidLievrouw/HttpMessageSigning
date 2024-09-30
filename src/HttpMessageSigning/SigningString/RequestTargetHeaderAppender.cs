using System;
using System.Text;
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

        public void Append(HeaderName header, StringBuilder sb) {
            var path = _request.RequestUri.OriginalString == "*"
                ? _request.RequestUri.OriginalString
                : _requestTargetEscaper.Escape(_request.RequestUri, _requestTargetEscaping);

            var headerToAppend = new Header(
                HeaderName.PredefinedHeaderNames.RequestTarget,
                $"{_request.Method.Method.ToLower()} {path}");
            headerToAppend.Append(sb);
        }
    }
}