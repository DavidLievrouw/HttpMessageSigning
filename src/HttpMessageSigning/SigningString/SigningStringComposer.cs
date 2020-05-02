using System;
using System.Linq;
using System.Text;

namespace Dalion.HttpMessageSigning.SigningString {
    internal class SigningStringComposer : ISigningStringComposer {
        private readonly IHeaderAppenderFactory _headerAppenderFactory;
        private readonly INonceAppender _nonceAppender;

        public SigningStringComposer(IHeaderAppenderFactory headerAppenderFactory, INonceAppender nonceAppender) {
            _headerAppenderFactory = headerAppenderFactory ?? throw new ArgumentNullException(nameof(headerAppenderFactory));
            _nonceAppender = nonceAppender ?? throw new ArgumentNullException(nameof(nonceAppender));
        }

        public string Compose(HttpRequestForSigning request, HeaderName[] headerNames, DateTimeOffset timeOfComposing, TimeSpan? expires, string nonce) {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (headerNames == null) throw new ArgumentNullException(nameof(headerNames));
            
            var headerAppender = _headerAppenderFactory.Create(request, timeOfComposing, expires);

            var sb = new StringBuilder();
            foreach (var headerName in headerNames.Where(h => h != HeaderName.Empty)) {
                sb = sb.Append(headerAppender.BuildStringToAppend(headerName));
            }

            sb.Append(_nonceAppender.BuildStringToAppend(nonce));
            
            return sb.ToString().Trim();
        }
    }
}