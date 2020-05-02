using System;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using Dalion.HttpMessageSigning.Signing;

namespace Conformance {
    public static class HttpRequestMessageParser {
        private static readonly string[] RestrictedHeaders = new[] {
            "Accept",
            "Connection",
            "Content-Length",
            "Content-Type",
            "Date",
            "Expect",
            "Host",
            "If-Modified-Since",
            "Keep-Alive",
            "Proxy-Connection",
            "Range",
            "Referer",
            "Transfer-Encoding",
            "User-Agent"
        };

        public static HttpRequestMessage Parse(string raw) {
            if (string.IsNullOrEmpty(raw)) throw new ArgumentException("Value cannot be null or empty.", nameof(raw));

            var request = new HttpRequestMessage();

            var lines = raw.Split('\n').Select(l => l.Trim()).ToList();
            for (var i = 0; i < lines.Count; i++) {
                var line = lines[i];
                var quit = false;
                switch (GetLineType(i, line)) {
                    case LineType.Definition:
                        var definitionParts = line.Split(' ');
                        request.Method = new HttpMethod(definitionParts[0]);
                        request.RequestUri = new Uri(definitionParts[1], UriKind.Relative);
                        request.Version = new Version(definitionParts[2].Split('/')[1]);
                        break;
                    case LineType.Header:
                        var header = line.Split(": ");
                        var values = RestrictedHeaders.Contains(header[0], StringComparer.OrdinalIgnoreCase)
                            ? new[] {header[1]}
                            : header[1].Split(", ");
                        request.Headers.Set(header[0], values.ToArray());
                        break;
                    case LineType.BodySeparator:
                        var bodyString = string.Join('\n', lines.Skip(i + 1));
                        request.Content = new StringContent(bodyString, Encoding.UTF8, MediaTypeNames.Application.Json);
                        quit = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (quit) break;
            }

            return request;
        }

        private static LineType GetLineType(int index, string line) {
            if (index == 0) return LineType.Definition;
            if (string.IsNullOrWhiteSpace(line)) return LineType.BodySeparator;
            return LineType.Header;
        }

        private enum LineType {
            Definition,
            Header,
            BodySeparator
        }
    }
}