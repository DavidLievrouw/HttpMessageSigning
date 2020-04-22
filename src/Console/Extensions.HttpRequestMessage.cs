using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning;
using Microsoft.AspNetCore.Http;

namespace Console {
    public static partial class Extensions {
        public static async Task<HttpRequest> ToServerSideHttpRequest(this HttpRequestMessage clientRequest) {
            if (clientRequest == null) return null;

            var request = new DefaultHttpContext().Request;
            request.Method = clientRequest.Method.Method;
            request.Scheme = clientRequest.RequestUri.IsAbsoluteUri ? clientRequest.RequestUri.Scheme : null;
            request.Host = clientRequest.RequestUri.IsAbsoluteUri ? new HostString(clientRequest.RequestUri.Host, clientRequest.RequestUri.Port) : new HostString();
            request.Path = clientRequest.RequestUri.IsAbsoluteUri ? clientRequest.RequestUri.AbsolutePath : clientRequest.RequestUri.OriginalString.Split('?')[0];
            request.Headers["Authorization"] = clientRequest.Headers.Authorization.Scheme + " " + clientRequest.Headers.Authorization.Parameter;

            var bodyTask = clientRequest.Content?.ReadAsStreamAsync();
            if (bodyTask != null) request.Body = await bodyTask;
            
            foreach (var header in clientRequest.Headers) {
                if (request.Headers.ContainsKey(header.Key)) {
                    request.Headers[header.Key] = clientRequest.Headers.GetValues(header.Key).ToArray();
                }
                else {
                    request.Headers.Add(header.Key, clientRequest.Headers.GetValues(header.Key).ToArray());
                }
            }

            return request;
        }
    }
}