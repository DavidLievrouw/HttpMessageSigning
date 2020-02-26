using System;
using System.Linq;
using System.Text;

namespace Dalion.HttpMessageSigning.SigningString {
    internal class SigningStringComposer : ISigningStringComposer {
        private readonly IHeaderAppenderFactory _headerAppenderFactory;

        public SigningStringComposer(IHeaderAppenderFactory headerAppenderFactory) {
            _headerAppenderFactory = headerAppenderFactory ?? throw new ArgumentNullException(nameof(headerAppenderFactory));
        }

        public string Compose(HttpRequestForSigning request, HeaderName[] headerNames, DateTimeOffset timeOfComposing, TimeSpan expires) {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (headerNames == null) throw new ArgumentNullException(nameof(headerNames));
            
            var headerAppender = _headerAppenderFactory.Create(request, timeOfComposing, expires);

            var sb = new StringBuilder();
            foreach (var headerName in headerNames.Where(h => h != HeaderName.Empty)) {
                sb = sb.Append(headerAppender.BuildStringToAppend(headerName));
            }

            return sb.ToString().Trim();
        }
    }
}