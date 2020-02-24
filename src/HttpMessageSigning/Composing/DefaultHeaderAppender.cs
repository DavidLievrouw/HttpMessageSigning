using System;
using System.Linq;
using System.Net.Http;

namespace Dalion.HttpMessageSigning.Composing {
    internal class DefaultHeaderAppender : IHeaderAppender {
        private readonly HttpRequestMessage _request;
        
        public DefaultHeaderAppender(HttpRequestMessage request) {
            _request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public string BuildStringToAppend(HeaderName header) {
            var isPresent = _request.Headers.TryGetValues(header, out var headerValues);
            var sanitizedHeaderValues = headerValues?.Select(SanitizeHeaderValue)?.ToArray();
            return isPresent
                ? "\n" + new Header(header, sanitizedHeaderValues)
                : string.Empty;
        }

        private static string SanitizeHeaderValue(string input) {
            var lines = input.Split(new []{'\n'}, StringSplitOptions.RemoveEmptyEntries);
            return string.Join(" ", lines.Select(l => l.Trim()));
        }
    }
}