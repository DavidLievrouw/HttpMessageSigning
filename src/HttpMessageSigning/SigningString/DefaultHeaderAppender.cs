using System;
using System.Linq;

namespace Dalion.HttpMessageSigning.SigningString {
    internal class DefaultHeaderAppender : IHeaderAppender {
        private readonly HttpRequestForSigning _request;
        
        public DefaultHeaderAppender(HttpRequestForSigning request) {
            _request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public string BuildStringToAppend(HeaderName header) {
            var isPresent = _request.Headers.TryGetValues(header, out var headerValues);
            
            if (!isPresent) throw new HttpMessageSigningException($"Header '{header}' was required to create the signature, but does not exist on the request message to sign.");
            
            return "\n" + new Header(header, headerValues.Select(SanitizeHeaderValue).ToArray());
        }

        private static string SanitizeHeaderValue(string input) {
            if (input == null) return null;
            if (input.IndexOf('\n') < 0) return input.Trim();
            var lines = input.Split(new []{'\n'}, StringSplitOptions.RemoveEmptyEntries);
            return string.Join(" ", lines.Select(l => l.Trim()));
        }
    }
}