using System.Linq;
using System.Net.Http;
using Microsoft.Extensions.Primitives;

namespace Dalion.HttpMessageSigning.Signing {
    public static partial class Extensions {
        internal static HttpRequestForSigning ToHttpRequestForSigning(this HttpRequestMessage httpRequestMessage) {
            if (httpRequestMessage == null) return null;

            var requestForSigning = new HttpRequestForSigning {
                Method = httpRequestMessage.Method,
                RequestUri = httpRequestMessage.RequestUri
            };

            foreach (var header in httpRequestMessage.Headers) {
                var values = header.Value.ToArray();
                requestForSigning.Headers[header.Key] = values.Length == 1
                    ? new StringValues(values[0])
                    : new StringValues(values);
            }

            if (httpRequestMessage.Content?.Headers?.Any() ?? false) {
                foreach (var contentHeader in httpRequestMessage.Content.Headers) {
                    var values = contentHeader.Value.ToArray();
                    requestForSigning.Headers[contentHeader.Key] = values.Length == 1
                        ? new StringValues(values[0])
                        : new StringValues(values);
                }
            }

            return requestForSigning;
        }
    }
}