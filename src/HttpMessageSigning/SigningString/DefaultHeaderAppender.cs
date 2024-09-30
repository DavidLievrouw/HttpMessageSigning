using System;
using System.Linq;
using System.Text;

namespace Dalion.HttpMessageSigning.SigningString {
    internal class DefaultHeaderAppender : IHeaderAppender {
        private readonly HttpRequestForSignatureString _request;
        
        public DefaultHeaderAppender(HttpRequestForSignatureString request) {
            _request = request ?? throw new ArgumentNullException(nameof(request));
        }
        
        public void Append(HeaderName header, StringBuilder sb) {
            var isPresent = _request.Headers.TryGetValues(header, out var headerValues);
            
            if (!isPresent) throw new HttpMessageSigningException($"Header '{header}' was required to create the signature, but does not exist on the request message to sign.");
            
            var headerToAppend = new Header(header, headerValues.Select(SanitizeHeaderValue).ToArray());
            headerToAppend.Append(sb);
        }

        private static string SanitizeHeaderValue(string input) {
            if (input == null) return null;
            if (input.IndexOf('\n') < 0) return input.Trim();

            var lines = input.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder();

            foreach (var line in lines) {
                if (sb.Length > 0) {
                    sb.Append(' ');
                }
                sb.Append(line.Trim());
            }

            return sb.ToString();
        }
    }
}