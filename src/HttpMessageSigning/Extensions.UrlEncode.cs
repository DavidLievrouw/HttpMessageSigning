using System;
using System.Linq;
using System.Text;

namespace Dalion.HttpMessageSigning {
    public static partial class Extensions {
        public static string UrlEncode(this Uri decoded) {
            if (decoded == null) return null;

            var decodedPath = decoded.IsAbsoluteUri
                ? decoded.AbsolutePath.UrlDecode()
                : decoded.OriginalString.UrlDecode();
            
            var segments = decodedPath
                .Split(new[] {"/"}, StringSplitOptions.None)
                .Select(EscapeSegment);

            string uriString = null;
            if (decoded.IsAbsoluteUri) {
                var sb = new StringBuilder();
                sb.Append(decoded.Scheme);
                sb.Append("://");
                sb.Append(decoded.Host);
                if (!decoded.IsDefaultPort) {
                    sb.Append(":");
                    sb.Append(decoded.Port);
                }
                sb.Append(string.Join("/", segments));
                uriString = sb.ToString();
            }
            else {
                uriString = string.Join("/", segments);
            }
            
            return uriString;
        }

        private static string EscapeSegment(string segment) {
            var encoded = Uri.EscapeDataString(segment);
            return encoded;
        }
    }
}