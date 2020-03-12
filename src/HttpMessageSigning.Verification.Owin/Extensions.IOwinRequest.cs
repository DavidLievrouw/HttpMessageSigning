using System.IO;
using System.Net.Http;
using System.Text;
using Microsoft.Owin;

namespace Dalion.HttpMessageSigning.Verification.Owin {
    public static partial class Extensions {
        internal static HttpRequestForSigning ToHttpRequestForSigning(this IOwinRequest owinRequest) {
            var request = new HttpRequestForSigning {
                Method = new HttpMethod(owinRequest.Method), 
                RequestUri = owinRequest.Uri
            };

            foreach (var header in owinRequest.Headers) {
                request.Headers[header.Key] = header.Value;
            }

            if (ShouldReadBody(owinRequest) && owinRequest.Body != null) {
                var bodyString = new StreamReader(owinRequest.Body).ReadToEnd();
                
                var requestData = Encoding.UTF8.GetBytes(bodyString);
                owinRequest.Body?.Dispose();
                owinRequest.Body = new MemoryStream(requestData);

                request.Body = bodyString;
            }

            return request;
        }

        private static bool ShouldReadBody(IOwinRequest request) {
            if (request.Body == null) return false;
            return request.Headers?.ContainsKey(HeaderName.PredefinedHeaderNames.Digest) ?? false;
        }
    }
}