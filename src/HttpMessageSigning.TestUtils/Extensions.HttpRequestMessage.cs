#if NETCORE
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Dalion.HttpMessageSigning.TestUtils {
    public static partial class Extensions {
        public static async Task<HttpRequest> ToServerSideHttpRequest(this HttpRequestMessage clientRequest) {
            if (clientRequest == null) return null;
            
            var absoluteUri = clientRequest.RequestUri.IsAbsoluteUri
                ? clientRequest.RequestUri
                : new Uri("https://dalion.eu" + clientRequest.RequestUri.OriginalString, UriKind.Absolute);

            var request = new DefaultHttpContext().Request;
            request.Method = clientRequest.Method.Method;
            request.Scheme = absoluteUri.Scheme;
            request.Host = clientRequest.RequestUri.IsAbsoluteUri
                ? new HostString(clientRequest.RequestUri.Host, clientRequest.RequestUri.Port)
                : new HostString();
            request.Path = new PathString(absoluteUri.AbsolutePath);
            request.Headers["Authorization"] = clientRequest.Headers.Authorization.Scheme + " " + clientRequest.Headers.Authorization.Parameter;
            
            if (clientRequest.RequestUri.IsAbsoluteUri) {
                request.QueryString = new Microsoft.AspNetCore.Http.QueryString(clientRequest.RequestUri.Query);
            }
            else {
                var originalString = clientRequest.RequestUri.OriginalString;
                var idx = originalString.IndexOf('?');
                var query = idx >= 0 ? originalString[idx..] : "";
                request.QueryString = new Microsoft.AspNetCore.Http.QueryString(query);
            }

            var bodyTask = clientRequest.Content?.ReadAsStreamAsync();
            if (bodyTask != null) request.Body = await bodyTask;

            foreach (var header in clientRequest.Headers) {
                if (request.Headers.ContainsKey(header.Key)) {
                    request.Headers[header.Key] = clientRequest.Headers.GetValues(header.Key).ToArray();
                }
                else {
                    request.Headers.Append(header.Key, clientRequest.Headers.GetValues(header.Key).ToArray());
                }
            }

            return request;
        }
    }
}
#endif

#if NETFRAMEWORK
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace Dalion.HttpMessageSigning.TestUtils {
    public static partial class Extensions {
        public static async Task<IOwinRequest> ToServerSideOwinRequest(this HttpRequestMessage clientRequest) {
            if (clientRequest == null) return null;

            var absoluteUri = clientRequest.RequestUri.IsAbsoluteUri
                ? clientRequest.RequestUri
                : new Uri("https://dalion.eu" + clientRequest.RequestUri.OriginalString, UriKind.Absolute);
            
            var request = new OwinRequest {
                Method = clientRequest.Method.Method,
                Scheme = clientRequest.RequestUri.IsAbsoluteUri ? clientRequest.RequestUri.Scheme : null,
                Host = clientRequest.RequestUri.IsAbsoluteUri
                    ? new HostString(clientRequest.RequestUri.Authority)
                    : new HostString(),
                Path = new PathString(absoluteUri.AbsolutePath)
            };
            
            request.Headers["Authorization"] = clientRequest.Headers.Authorization.Scheme + " " + clientRequest.Headers.Authorization.Parameter;

            if (clientRequest.RequestUri.IsAbsoluteUri) {
                request.QueryString = string.IsNullOrEmpty(clientRequest.RequestUri.Query)
                    ? QueryString.Empty
                    : new QueryString(clientRequest.RequestUri.Query.Substring(1));
            }
            else {
                var originalString = clientRequest.RequestUri.OriginalString;
                var idx = originalString.IndexOf('?');
                var query = idx >= 0 ? originalString.Substring(idx + 1) : "";
                request.QueryString = new QueryString(query);
            }

            var bodyTask = clientRequest.Content?.ReadAsStreamAsync();
            if (bodyTask != null) request.Body = await bodyTask;

            foreach (var header in clientRequest.Headers) {
                request.Headers.SetValues(header.Key, clientRequest.Headers.GetValues(header.Key).ToArray());
            }

            return request;
        }
    }
}
#endif