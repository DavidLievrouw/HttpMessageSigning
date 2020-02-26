using System;

namespace Dalion.HttpMessageSigning.SigningString {
    internal class DateHeaderAppender : IHeaderAppender {
        private readonly HttpRequestForSigning _request;
        
        public DateHeaderAppender(HttpRequestForSigning request) {
            _request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public string BuildStringToAppend(HeaderName header) {
            var dateValue = _request.Headers.Date;
            return dateValue.HasValue
                ? "\n" + new Header(HeaderName.PredefinedHeaderNames.Date, dateValue.Value.ToString("R"))
                : string.Empty;
        }
    }
}