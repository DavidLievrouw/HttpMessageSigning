using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Owin;

namespace Dalion.HttpMessageSigning.Verification.Owin {
    public static partial class Extensions {
        internal static HttpRequest ToHttpRequest(this IOwinRequest owinRequest) {
            var request = new DefaultHttpContext().Request;

            request.Method = owinRequest.Method;
            request.Scheme = owinRequest.Scheme;
            request.Host = new Microsoft.AspNetCore.Http.HostString(owinRequest.Host.Value);
            if (owinRequest.PathBase.HasValue) request.PathBase = new Microsoft.AspNetCore.Http.PathString(owinRequest.PathBase.Value);
            if (owinRequest.Path.HasValue) request.Path = new Microsoft.AspNetCore.Http.PathString(owinRequest.Path.Value);

            foreach (var header in owinRequest.Headers) {
                request.Headers[header.Key] = header.Value;
            }

            if (ShouldReadBody(owinRequest) && owinRequest.Body != null) {
                var bodyString = new StreamReader(owinRequest.Body).ReadToEnd();
                var requestData = Encoding.UTF8.GetBytes(bodyString);

                owinRequest.Body?.Dispose();
                owinRequest.Body = new MemoryStream(requestData);

                request.Body = new MemoryStream(requestData);
            }

            return request;
        }

        private static bool ShouldReadBody(IOwinRequest request) {
            if (request.Body == null) return false;
            return request.Headers?.ContainsKey(HeaderName.PredefinedHeaderNames.Digest) ?? false;
        }
    }
}