using System;
using System.Net.Http;

namespace Dalion.HttpMessageSigning {
    internal class HttpRequestForSigning {
        public HttpMethod Method { get; set; } = HttpMethod.Get;
        public Uri RequestUri { get; set; }
        public HeaderDictionary Headers { get; set; } = new HeaderDictionary();
        public string SignatureAlgorithmName { get; set; }
        public string Body { get; set; }
    }
}