using System;
using System.Net.Http;

namespace Dalion.HttpMessageSigning.Composing {
    internal class DateHeaderAppender : IHeaderAppender {
        private readonly HttpRequestMessage _request;
        
        public DateHeaderAppender(HttpRequestMessage request) {
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