using System;
using System.Linq;
using System.Net.Http;
using Microsoft.Extensions.Primitives;

namespace Dalion.HttpMessageSigning {
    public static partial class Extensions {
        internal static HttpRequestForSigning ToHttpRequestForSigning(this HttpRequestMessage httpRequestMessage) {
            if (httpRequestMessage == null) return null;

            var absoluteUri = httpRequestMessage.RequestUri.IsAbsoluteUri
                ? httpRequestMessage.RequestUri
                : new Uri("https://dalion.eu" + httpRequestMessage.RequestUri.OriginalString, UriKind.Absolute);
            
            var requestForSigning = new HttpRequestForSigning {
                Method = httpRequestMessage.Method,
                RequestUri = new Uri("https://dalion.eu" + absoluteUri.GetComponents(UriComponents.PathAndQuery, UriFormat.Unescaped))
                    .GetComponents(UriComponents.PathAndQuery, UriFormat.UriEscaped)
            };

            foreach (var header in httpRequestMessage.Headers) {
                requestForSigning.Headers[header.Key] = new StringValues(header.Value.ToArray());
            }
            
            if (httpRequestMessage.Content?.Headers?.Any() ?? false) {
                foreach (var contentHeader in httpRequestMessage.Content.Headers) {
                    requestForSigning.Headers[contentHeader.Key] = new StringValues(contentHeader.Value.ToArray());
                }
            }
            
            return requestForSigning;
        }
    }
}