using System;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace Dalion.HttpMessageSigning.Verification {
    public static partial class Extensions {
        internal static HttpRequestMessage ToHttpRequestMessage(this HttpRequest request) {
            if (request == null) return null;
            
            var requestMessage = new HttpRequestMessage {
                RequestUri = new Uri(request.GetEncodedUrl(), UriKind.Absolute),
                Method = new HttpMethod(request.Method)
            };
            
            if (requestMessage.Method.SupportsBody() && request.Body != null) {
                var streamContent = new StreamContent(request.Body);
                requestMessage.Content = streamContent;
            }

            foreach (var header in request.Headers) {
                if (!requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray())) {
                    requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                }
            }

            requestMessage.Headers.Host = requestMessage.RequestUri.Authority;
            
            return requestMessage;
        }
    }
}