using System;
using System.Net.Http;

namespace Dalion.HttpMessageSigning {
    internal class HttpRequestForSigning : ICloneable {
        public HttpMethod Method { get; set; } = HttpMethod.Get;
        public Uri RequestUri { get; set; }
        public HeaderDictionary Headers { get; set; } = new HeaderDictionary();
        public string SignatureAlgorithmName { get; set; }
        public string Body { get; set; }
        
        public object Clone() {
            return new HttpRequestForSigning {
                Method = Method,
                RequestUri = RequestUri,
                Body = Body,
                Headers = Headers == null ? null : new HeaderDictionary(Headers.ToDictionary()),
                SignatureAlgorithmName = SignatureAlgorithmName
            };
        }
    }
}