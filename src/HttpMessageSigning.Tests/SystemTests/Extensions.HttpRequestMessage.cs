using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Dalion.HttpMessageSigning.SystemTests {
    public static partial class Extensions {
        public static async Task<HttpRequest> ToServerSideHttpRequest(this HttpRequestMessage clientRequest) {
            if (clientRequest == null) return null;

            var request = new DefaultHttpContext().Request;
            request.Method = clientRequest.Method.Method;
            request.Scheme = clientRequest.RequestUri.Scheme;
            request.Host = new HostString(clientRequest.RequestUri.Host, clientRequest.RequestUri.Port);
            request.Path = clientRequest.RequestUri.LocalPath;
            request.Headers["Authorization"] = clientRequest.Headers.Authorization.Scheme + " " + clientRequest.Headers.Authorization.Parameter;

            var bodyTask = clientRequest.Content?.ReadAsStreamAsync();
            if (bodyTask != null) request.Body = await bodyTask;

            if (clientRequest.Headers.Contains("Dalion-App-Id")) {
                request.Headers.Add("Dalion-App-Id", clientRequest.Headers.GetValues("Dalion-App-Id").ToArray());
            }

            if (clientRequest.Headers.Contains(HeaderName.PredefinedHeaderNames.Digest)) {
                request.Headers.Add(HeaderName.PredefinedHeaderNames.Digest, clientRequest.Headers.GetValues(HeaderName.PredefinedHeaderNames.Digest).ToArray());
            }

            if (clientRequest.Headers.Contains(HeaderName.PredefinedHeaderNames.Date)) {
                request.Headers.Add(HeaderName.PredefinedHeaderNames.Date, clientRequest.Headers.GetValues(HeaderName.PredefinedHeaderNames.Date).ToArray());
            }

            return request;
        }
    }
}