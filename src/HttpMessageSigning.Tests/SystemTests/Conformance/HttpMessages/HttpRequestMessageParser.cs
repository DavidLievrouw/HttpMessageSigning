using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using Dalion.HttpMessageSigning.Signing;

namespace Dalion.HttpMessageSigning.SystemTests.Conformance.HttpMessages {
    public static class HttpRequestMessageParser {
        private static readonly string[] RestrictedHeaders = {
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

        private static readonly string[] ContentHeaders = {
            "Content-Length",
            "Content-Type",
            "Content-Language",
            "Content-Encoding",
            "Content-Location",
            "Content-Disposition"
        };

        public static HttpRequestMessage Parse(string raw) {
            if (string.IsNullOrEmpty(raw)) throw new ArgumentException("Value cannot be null or empty.", nameof(raw));

            var request = new HttpRequestMessage();

            var lines = raw.Split('\n').Select(l => l.Trim()).ToList();
            var contentHeaders = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < lines.Count; i++) {
                var line = lines[i];
                var quit = false;
                switch (GetLineType(i, line)) {
                    case LineType.Definition:
                        var definitionParts = line.Split(' ', 3);
                        request.Method = new HttpMethod(definitionParts[0]);
                        request.RequestUri = new Uri(definitionParts[1], UriKind.Relative);
                        request.Version = new Version(definitionParts[2].Split('/', 2)[1]);
                        break;
                    case LineType.Header:
                        var header = line.Split(": ", 2);
                        var values = RestrictedHeaders.Contains(header[0], StringComparer.OrdinalIgnoreCase)
                            ? new[] {header.Length > 1 ? header[1] : string.Empty}
                            : header.Length > 1
                                ? header[1].Split(", ")
                                : new[] {""};
                        if (ContentHeaders.Contains(header[0], StringComparer.OrdinalIgnoreCase)) {
                            contentHeaders.Add(header[0].TrimEnd(':'), values);
                        }
                        else {
                            request.Headers.Set(header[0].TrimEnd(':'), values.ToArray());
                        }

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

            if (request.Content != null && contentHeaders.Any()) {
                foreach (var contentHeader in contentHeaders) {
                    request.Content.Headers.Set(contentHeader.Key, contentHeader.Value);
                }
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