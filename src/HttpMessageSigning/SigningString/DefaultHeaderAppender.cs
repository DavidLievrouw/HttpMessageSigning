using System;
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

            var values = new string[headerValues.Count];
            for (var i = 0; i < headerValues.Count; i++) {
                values[i] = SanitizeHeaderValue(headerValues[i]);
            }

            var headerToAppend = new Header(header, values);
            headerToAppend.Append(sb);
        }

#if NET8_0_OR_GREATER
        private static string SanitizeHeaderValue(string input) {
            if (input == null) return null;
            if (input.IndexOf('\n') < 0) return input.Trim();

            var sb = new StringBuilder(input.Length);
            var span = input.AsSpan();
            var inWhitespace = true;

            foreach (var c in span) {
                if (c == '\n') {
                    if (!inWhitespace) {
                        sb.Append(' ');
                        inWhitespace = true;
                    }
                }
                else if (char.IsWhiteSpace(c)) {
                    if (!inWhitespace) {
                        sb.Append(' ');
                        inWhitespace = true;
                    }
                }
                else {
                    sb.Append(c);
                    inWhitespace = false;
                }
            }

            if (sb.Length > 0 && sb[^1] == ' ') {
                sb.Length--;
            }

            return sb.ToString();
        }
#else
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
#endif
    }
}