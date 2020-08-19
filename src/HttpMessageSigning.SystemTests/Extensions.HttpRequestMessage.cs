using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Dalion.HttpMessageSigning {
    public static partial class Extensions {
        public static async Task<HttpRequest> ToServerSideHttpRequest(this HttpRequestMessage clientRequest) {
            if (clientRequest == null) return null;

            var request = new DefaultHttpContext().Request;
            request.Method = clientRequest.Method.Method;
            request.Scheme = clientRequest.RequestUri.IsAbsoluteUri ? clientRequest.RequestUri.Scheme : null;
            request.Host = clientRequest.RequestUri.IsAbsoluteUri ? new HostString(clientRequest.RequestUri.Host, clientRequest.RequestUri.Port) : new HostString();
            request.Path = clientRequest.RequestUri.IsAbsoluteUri ? clientRequest.RequestUri.AbsolutePath : clientRequest.RequestUri.OriginalString.Split('?')[0];
            request.Headers["Authorization"] = clientRequest.Headers.Authorization.Scheme + " " + clientRequest.Headers.Authorization.Parameter;

            if (clientRequest.RequestUri.IsAbsoluteUri) {
                request.QueryString = new Microsoft.AspNetCore.Http.QueryString(clientRequest.RequestUri.Query);
            }
            else {
                var originalString = clientRequest.RequestUri.OriginalString;
                var idx = originalString.IndexOf('?');
                var query = idx >= 0 ? originalString.Substring(idx) : "";
                request.QueryString = new Microsoft.AspNetCore.Http.QueryString(query);
            }

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