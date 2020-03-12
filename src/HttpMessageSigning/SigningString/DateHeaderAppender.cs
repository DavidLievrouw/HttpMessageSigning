using System;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Primitives;

namespace Dalion.HttpMessageSigning.SigningString {
    internal class DateHeaderAppender : IHeaderAppender {
        private readonly HttpRequestForSigning _request;
        
        public DateHeaderAppender(HttpRequestForSigning request) {
            _request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public string BuildStringToAppend(HeaderName header) {
            var dateValues = _request.Headers[HeaderName.PredefinedHeaderNames.Date];
            
            if (dateValues == StringValues.Empty) return string.Empty;
            if (!DateTimeOffset.TryParseExact(dateValues.First(), "R", CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.None, out var dateValue)) {
                return string.Empty;
            }
            
            return "\n" + new Header(HeaderName.PredefinedHeaderNames.Date, dateValue.ToString("R"));
        }
    }
}