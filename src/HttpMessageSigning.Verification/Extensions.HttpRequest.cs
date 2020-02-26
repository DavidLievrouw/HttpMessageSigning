using System;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace Dalion.HttpMessageSigning.Verification {
    public static partial class Extensions {
        internal static HttpRequestForSigning ToRequestForSigning(this HttpRequest request, Signature signature, Client client) {
            if (signature == null) throw new ArgumentNullException(nameof(signature));
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (request == null) return null;

            var requestMessage = new HttpRequestForSigning {
                SignatureAlgorithmName = client.SignatureAlgorithm.Name,
                RequestUri = new Uri(request.GetEncodedUrl(), UriKind.Absolute),
                Method = string.IsNullOrEmpty(request.Method)
                    ? HttpMethod.Get
                    : new HttpMethod(request.Method)
            };

            foreach (var header in request.Headers) {
                requestMessage.Headers[header.Key] = header.Value;
            }

            if (signature.Expires.HasValue && signature.Created.HasValue) {
                requestMessage.Expires = signature.Expires.Value - signature.Created.Value;
            }

            return requestMessage;
        }
    }
}