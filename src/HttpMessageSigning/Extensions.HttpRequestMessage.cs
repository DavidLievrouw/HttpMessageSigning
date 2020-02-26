using System;
using System.Linq;
using System.Net.Http;

namespace Dalion.HttpMessageSigning {
    public static partial class Extensions {
        internal static HttpRequestForSigning ToRequestForSigning(this HttpRequestMessage httpRequestMessage) {
            if (httpRequestMessage == null) throw new ArgumentNullException(nameof(httpRequestMessage));

            var unifiedHeaders = httpRequestMessage.Headers;
            if (httpRequestMessage.Content?.Headers?.Any() ?? false) {
                foreach (var contentHeader in httpRequestMessage.Content.Headers) {
                    unifiedHeaders.TryAddWithoutValidation(contentHeader.Key, contentHeader.Value);
                }
            }
            
            return new HttpRequestForSigning {
                Method = httpRequestMessage.Method,
                RequestUri = httpRequestMessage.RequestUri,
                Headers = unifiedHeaders
            };
        }
    }
}