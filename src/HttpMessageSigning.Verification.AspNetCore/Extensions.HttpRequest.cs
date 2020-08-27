using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Primitives;

namespace Dalion.HttpMessageSigning.Verification.AspNetCore {
    public static partial class Extensions {
        internal static async Task<HttpRequestForVerification> ToHttpRequestForVerification(this HttpRequest request, Signature signature) {
            if (signature == null) throw new ArgumentNullException(nameof(signature));
            if (request == null) return null;
            
            var requestMessage = new HttpRequestForVerification {
                RequestUri = GetRequestUri(request),
                Method = string.IsNullOrEmpty(request.Method)
                    ? HttpMethod.Get
                    : new HttpMethod(request.Method),
                Signature = signature
            };

            foreach (var header in request.Headers) {
                requestMessage.Headers[header.Key] = new StringValues((string[])header.Value);
            }

            if (ShouldReadBody(request, signature) && request.Body != null) {
                request.EnableBuffering();

                // ReSharper disable once UseAwaitUsing
                // ReSharper disable once ConvertToUsingDeclaration
                using (var memoryStream = new MemoryStream()) {
                    await request.Body.CopyToAsync(memoryStream).ConfigureAwait(false);
                    requestMessage.Body = memoryStream.ToArray();
                    request.Body.Seek(0, SeekOrigin.Begin);
                }
            }

            return requestMessage;
        }

        private static bool ShouldReadBody(HttpRequest request, Signature signature) {
            if (request.Body == null) return false;
            return (signature.Headers?.Contains(HeaderName.PredefinedHeaderNames.Digest) ?? false) || 
                   (request.Headers?.ContainsKey(HeaderName.PredefinedHeaderNames.Digest) ?? false);
        }

        private static string GetRequestUri(HttpRequest request) {
            var uri = new Uri(request.GetDisplayUrl());
            return new Uri("https://dalion.eu" + uri.GetComponents(UriComponents.PathAndQuery, UriFormat.Unescaped), UriKind.Absolute)
                .GetComponents(UriComponents.PathAndQuery, UriFormat.UriEscaped);
        }
    }
}