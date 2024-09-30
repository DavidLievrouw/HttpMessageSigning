using System;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Primitives;

namespace Dalion.HttpMessageSigning.SigningString {
    internal class DateHeaderAppender : IHeaderAppender {
        private readonly HttpRequestForSignatureString _request;
        
        public DateHeaderAppender(HttpRequestForSignatureString request) {
            _request = request ?? throw new ArgumentNullException(nameof(request));
        }
        
        public void Append(HeaderName header, StringBuilder sb) {
            var dateValues = _request.Headers[HeaderName.PredefinedHeaderNames.Date];
            
            if (dateValues == StringValues.Empty) return;
            if (!DateTimeOffset.TryParseExact(dateValues.First(), "R", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateValue)) {
                return;
            }
            
            var headerToAppend = new Header(HeaderName.PredefinedHeaderNames.Date, dateValue.ToString("R"));
            headerToAppend.Append(sb);
        }
    }
}