using System;
using System.Net.Http;
using Dalion.HttpMessageSigning.SigningString;

namespace Dalion.HttpMessageSigning.Signing {
    internal class HttpRequestForSigning : ICloneable {
        public HttpMethod Method { get; set; } = HttpMethod.Get;
        public Uri RequestUri { get; set; }
        public HeaderDictionary Headers { get; set; } = new HeaderDictionary();

        public object Clone() {
            return new HttpRequestForSigning {
                Method = Method,
                RequestUri = RequestUri,
                Headers = Headers == null ? null : new HeaderDictionary(Headers.ToDictionary())
            };
        }

        internal HttpRequestForSignatureString ToHttpRequestForSignatureString() {
            return new HttpRequestForSignatureString {
                Method = Method,
                RequestUri = RequestUri,
                Headers = Headers == null ? null : new HeaderDictionary(Headers.ToDictionary())
            };
        }
    }
}