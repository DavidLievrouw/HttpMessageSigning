using System.IO;
using System.Net.Http;
using Microsoft.Owin;

namespace Dalion.HttpMessageSigning.Verification.Owin {
    public static partial class Extensions {
        internal static HttpRequestForSigning ToHttpRequestForSigning(this IOwinRequest owinRequest) {
            var request = new HttpRequestForSigning {
                Method = new HttpMethod(owinRequest.Method), 
                RequestUri = owinRequest.Uri.AbsolutePath
            };

            foreach (var header in owinRequest.Headers) {
                request.Headers[header.Key] = header.Value;
            }

            if (ShouldReadBody(owinRequest) && owinRequest.Body != null) {
                using (var memoryStream = new MemoryStream()) {
                    owinRequest.Body.CopyTo(memoryStream);
                    request.Body = memoryStream.ToArray();
                    
                    owinRequest.Body?.Dispose();
                    owinRequest.Body = new MemoryStream(request.Body);
                }
            }

            return request;
        }

        private static bool ShouldReadBody(IOwinRequest request) {
            if (request.Body == null) return false;
            return request.Headers?.ContainsKey(HeaderName.PredefinedHeaderNames.Digest) ?? false;
        }
    }
}