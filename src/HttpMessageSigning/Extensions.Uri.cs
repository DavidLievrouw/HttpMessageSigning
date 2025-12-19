using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dalion.HttpMessageSigning {
    public static partial class Extensions {
        /// <summary>
        ///     Perform URI escaping, according to RFC 3986, on the specified <see cref="Uri" />.
        /// </summary>
        /// <param name="decoded">The <see cref="Uri" /> that needs to be escaped.</param>
        /// <param name="escaping">The rule set to follow when escaping.</param>
        /// <returns>The escaped string representation of the specified <see cref="Uri" />, according to RFC 3986.</returns>
        public static string UriEscape(this Uri decoded, UriEscaping escaping = UriEscaping.RFC3986) {
            switch (escaping) {
                case UriEscaping.RFC3986:
                    return decoded.UriEscapeRFC3986();
                case UriEscaping.RFC2396:
                    return decoded.UriEscapeRFC2396();
                default:
                    throw new ArgumentOutOfRangeException(nameof(escaping), escaping, $"The specified {nameof(UriEscaping)} value is currently not supported.");
            }
        }

        private static string UriEscapeRFC3986(this Uri decoded) {
            if (decoded == null) return null;

            var isAbsolute = decoded.IsAbsoluteUri;
            var originalString = decoded.OriginalString;

            var decodedPath = isAbsolute
                ? decoded.GetComponents(UriComponents.Path, UriFormat.Unescaped).TrimStart('/')
                : FastSplitInTwo(originalString, separator: '?')[0].TrimStart('/');

            string decodedQuery = null;
            if (originalString.IndexOf(value: '?') > -1) {
                decodedQuery = isAbsolute
                    ? decoded.GetComponents(UriComponents.Query, UriFormat.Unescaped)
                    : FastSplitInTwo(FastSplitInTwo(originalString, separator: '?')[1], separator: '#')[0];
            }

            var pathSegments = FastSplit(decodedPath, separator: '/').Select(Uri.EscapeDataString);

            var queryString = ExtractQueryString(decodedQuery)?.ToString();

            var sb = new StringBuilder();
            if (isAbsolute) {
                sb.Append(decoded.Scheme);
                sb.Append("://");
                sb.Append(decoded.Host);
                if (!decoded.IsDefaultPort) {
                    sb.Append(value: ':');
                    sb.Append(decoded.Port);
                }

                sb.Append(value: '/');
            }

            if (!isAbsolute) sb.Append(value: '/');
            sb.Append(string.Join("/", pathSegments));

            if (!string.IsNullOrEmpty(queryString)) {
                sb.Append("?" + queryString);
            }

            return sb.ToString();
        }

        private static string UriEscapeRFC2396(this Uri decoded) {
            if (decoded == null) return null;

            if (decoded.IsAbsoluteUri) {
                return new Uri(decoded.OriginalString.UriUnescape())
                    .GetComponents(UriComponents.HttpRequestUrl, UriFormat.UriEscaped);
            }

            var originalString = decoded.OriginalString;
            var withoutHash = originalString.IndexOf(value: '#') < 0
                ? originalString
                : FastSplitInTwo(originalString, separator: '#')[0];

            var absoluteUri = new Uri("https://dalion.eu/" + withoutHash.TrimStart('/'), UriKind.Absolute);

            return absoluteUri.GetComponents(UriComponents.PathAndQuery, UriFormat.UriEscaped);
        }

        private static StringBuilder ExtractQueryString(string decodedQuery) {
            if (decodedQuery == null) return null;
            if (decodedQuery == string.Empty) return null;

            var query = decodedQuery.IndexOf(value: '#') < 0
                ? decodedQuery
                : FastSplitInTwo(decodedQuery, separator: '#')[0];

            var pairs = query.IndexOf(value: '&') < 0
                ? (object)query
                : (object)FastSplit(query, separator: '&');

            var sb = new StringBuilder();

            if (pairs is string str) {
                var parts = FastSplitInTwo(str, separator: '=');

                sb.Append(Uri.EscapeDataString(parts[0]));
                if (parts.Length > 1) {
                    sb.Append('=');
                    sb.Append(Uri.EscapeDataString(parts[1]));
                }

                return sb;
            }

            var isFirst = true;
            foreach (var pair in (IEnumerable<string>)pairs) {
                var parts = FastSplitInTwo(pair, separator: '=');

                if (!isFirst) sb.Append('&');

                sb.Append(Uri.EscapeDataString(parts[0]));
                if (parts.Length > 1) {
                    sb.Append('=');
                    sb.Append(Uri.EscapeDataString(parts[1]));
                }

                isFirst = false;
            }

            return sb;
        }

        private static List<string> FastSplit(string input, char separator) {
            var span = input.AsSpan();

            var index = span.IndexOf(separator);

            var items = new List<string>();

#if NET8_0_OR_GREATER
            while (index > -1) {
                var item = span[..index].ToString();
                span = span[(index + 1)..];
                items.Add(item);
                index = span.IndexOf(separator);
            }
#else
            while (index > -1) {
                var item = span.Slice(0, index).ToString();
                span = span.Slice(index + 1);
                items.Add(item);
                index = span.IndexOf(separator);
            }
#endif
            items.Add(span.ToString());

            return items;
        }

        private static string[] FastSplitInTwo(string input, char separator) {
            var span = input.AsSpan();

            var index = span.IndexOf(separator);
            if (index < 0) return new[] { input };

#if NET8_0_OR_GREATER
            var part1 = span[..index].ToString();
            span = span[(index + 1)..];
#else
            var part1 = span.Slice(0, index).ToString();
            span = span.Slice(index + 1);
#endif

            var part2 = span.ToString();

            return new[] { part1, part2 };
        }
    }
}