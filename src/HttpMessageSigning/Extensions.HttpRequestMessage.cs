using System;
using System.Linq;
using System.Net.Http;
using Microsoft.Extensions.Primitives;

namespace Dalion.HttpMessageSigning {
    public static partial class Extensions {
        internal static HttpRequestForSigning ToHttpRequestForSigning(this HttpRequestMessage httpRequestMessage) {
            if (httpRequestMessage == null) return null;
            
            var requestForSigning = new HttpRequestForSigning {
                Method = httpRequestMessage.Method,
                RequestUri = httpRequestMessage.RequestUri.IsAbsoluteUri
                 ? httpRequestMessage.RequestUri.GetComponents(UriComponents.PathAndQuery, UriFormat.Unescaped).UrlEncode()
                 : httpRequestMessage.RequestUri.OriginalString.UrlEncode()
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