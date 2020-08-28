using System;
using System.Linq;
using System.Text;

namespace Dalion.HttpMessageSigning {
    public static partial class Extensions {
        /// <summary>
        ///     Perform URI encoding, according to RFC 3986, on the specified <see cref="Uri" />.
        /// </summary>
        /// <param name="decoded">The <see cref="Uri" /> that needs to be encoded.</param>
        /// <returns>The encoded string representation of the specified <see cref="Uri" />, according to RFC 3986.</returns>
        public static string UrlEncode(this Uri decoded) {
            if (decoded == null) return null;

            var isAbsolute = decoded.IsAbsoluteUri;
            var absoluteUri = isAbsolute
                ? decoded
                : new Uri("https://dalion.eu/" + decoded.OriginalString.TrimStart('/'), UriKind.Absolute);
            var decodedPath = absoluteUri.GetComponents(UriComponents.Path, UriFormat.Unescaped);
            var decodedQuery = absoluteUri.GetComponents(UriComponents.Query, UriFormat.Unescaped);

            var pathSegments = decodedPath
                .Split(new[] {"/"}, StringSplitOptions.None)
                .Select(Uri.EscapeDataString);
            var path = string.Join("/", pathSegments);

            var queryStringCollection = ExtractQueryString(decodedQuery);
            var qsSegments = queryStringCollection.AllKeys
                .Select(key => {
                    var val = queryStringCollection[key];
                    return string.IsNullOrEmpty(val)
                        ? Uri.EscapeDataString(key)
                        : Uri.EscapeDataString(key) + "=" + Uri.EscapeDataString(val);
                });
            var queryString = string.Join("&", qsSegments);

            var sb = new StringBuilder();
            if (isAbsolute) {
                sb.Append(decoded.Scheme);
                sb.Append("://");
                sb.Append(decoded.Host);
                if (!decoded.IsDefaultPort) {
                    sb.Append(':');
                    sb.Append(decoded.Port);
                }
            }

            if (isAbsolute || decoded.OriginalString.StartsWith("/", StringComparison.Ordinal)) sb.Append('/');
            sb.Append(path);
            if (!string.IsNullOrEmpty(queryString)) {
                sb.Append("?" + queryString);
            }

            return sb.ToString();
        }

        private static System.Collections.Specialized.NameValueCollection ExtractQueryString(string decodedQuery) {
            var result = new System.Collections.Specialized.NameValueCollection(capacity: 4);
            if (string.IsNullOrEmpty(decodedQuery)) return result;

            var query = decodedQuery.Split('#')[0];
            var pairs = query.Split('&');
            foreach (var pair in pairs) {
                var parts = pair.Split(new[] {'='}, count: 2);
                var name = Uri.UnescapeDataString(parts[0]);
                var value = parts.Length == 1 ? string.Empty : parts[1];
                result.Add(name, value);
            }

            return result;
        }
    }
}