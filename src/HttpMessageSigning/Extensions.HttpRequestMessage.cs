using System;
using System.Net.Http;

namespace Dalion.HttpMessageSigning {
    public static partial class Extensions {
        internal static HttpRequestForSigning ToRequestForSigning(this HttpRequestMessage httpRequestMessage) {
            if (httpRequestMessage == null) throw new ArgumentNullException(nameof(httpRequestMessage));
            
            return new HttpRequestForSigning {
                Method = httpRequestMessage.Method,
                RequestUri = httpRequestMessage.RequestUri,
                Headers = httpRequestMessage.Headers
            };
        }
    }
}