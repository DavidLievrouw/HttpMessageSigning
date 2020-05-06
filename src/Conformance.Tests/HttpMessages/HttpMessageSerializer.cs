using System;
using System.Net.Http;
using System.Text;

namespace Dalion.HttpMessageSigning.HttpMessages {
    public static class HttpMessageSerializer {
        public static string Serialize(HttpRequestMessage request) {
            if (request == null) throw new ArgumentNullException(nameof(request));
            
            var sb = new StringBuilder();
            sb.AppendFormat("{0} {1} HTTP/{2}\n", request.Method.Method.ToUpper(), request.RequestUri, request.Version.ToString(2));
            
            foreach (var header in request.Headers) {
                sb.AppendFormat("{0}: {1}\n", header.Key, string.Join(", ", header.Value));
            }

            if (request.Content != null) {
                foreach (var header in request.Content.Headers) {
                    sb.AppendFormat("{0}: {1}\n", header.Key, string.Join(", ", header.Value));
                }

                var bodyString = request.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                if (!string.IsNullOrEmpty(bodyString)) {
                    sb.AppendFormat("\n{0}", bodyString);
                }
            }
            
            return sb.ToString().TrimEnd('\n');
        }
    }
}