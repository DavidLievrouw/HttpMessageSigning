using System;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace Dalion.HttpMessageSigning.Validation {
    public static class Extensions {
        internal static HttpRequestMessage ToHttpRequestMessage(this HttpRequest request) {
            var requestMessage = new HttpRequestMessage {
                RequestUri = new Uri(request.GetEncodedUrl(), UriKind.Absolute),
                Method = new HttpMethod(request.Method)
            };
            
            requestMessage.Headers.Host = requestMessage.RequestUri.Authority;
            
            if (requestMessage.Method.HasBody()) {
                var streamContent = new StreamContent(request.Body);
                requestMessage.Content = streamContent;
            }

            foreach (var header in request.Headers) {
                if (!requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray())) {
                    requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                }
            }

            return requestMessage;
        }
    }
}