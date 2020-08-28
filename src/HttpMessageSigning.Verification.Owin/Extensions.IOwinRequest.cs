using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using Microsoft.Owin;

namespace Dalion.HttpMessageSigning.Verification.Owin {
    /// <summary>
    ///     Extension methods for this library.
    /// </summary>
    public static partial class Extensions {
        internal static HttpRequestForVerification ToHttpRequestForVerification(this IOwinRequest owinRequest, Signature signature) {
            if (owinRequest == null) return null;
            if (signature == null) throw new ArgumentNullException(nameof(signature));
            
            var request = new HttpRequestForVerification {
                Method = new HttpMethod(owinRequest.Method),
                RequestUri = owinRequest.Uri.GetComponents(UriComponents.PathAndQuery, UriFormat.Unescaped).UrlEncode(),
                Signature = signature
            };

            foreach (var header in owinRequest.Headers) {
                request.Headers[header.Key] = header.Value;
            }

            if (ShouldReadBody(owinRequest, signature) && owinRequest.Body != null) {
                using (var memoryStream = new MemoryStream()) {
                    owinRequest.Body.CopyTo(memoryStream);
                    request.Body = memoryStream.ToArray();

                    owinRequest.Body?.Dispose();
                    owinRequest.Body = new MemoryStream(request.Body);
                }
            }

            return request;
        }

        
        private static bool ShouldReadBody(IOwinRequest request, Signature signature) {
            if (request.Body == null) return false;
            return (signature.Headers?.Any(h => h == HeaderName.PredefinedHeaderNames.Digest) ?? false) || 
                   (request.Headers?.ContainsKey(HeaderName.PredefinedHeaderNames.Digest) ?? false);
        }
    }
}