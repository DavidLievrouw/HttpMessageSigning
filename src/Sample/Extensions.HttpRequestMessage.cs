using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;

namespace Sample {
    public static partial class Extensions {
        public static async Task<HttpRequest> ToServerSideHttpRequest(this HttpRequestMessage clientRequest) {
            if (clientRequest == null) return null;

            var request = new DefaultHttpRequest(new DefaultHttpContext()) {
                Method = clientRequest.Method.Method,
                Scheme = clientRequest.RequestUri.Scheme,
                Host = new HostString(clientRequest.RequestUri.Host, clientRequest.RequestUri.Port),
                Path = clientRequest.RequestUri.LocalPath,
                Headers = {
                    {"Authorization", clientRequest.Headers.Authorization.Scheme + " " + clientRequest.Headers.Authorization.Parameter}
                }
            };

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