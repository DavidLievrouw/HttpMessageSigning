using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Dalion.HttpMessageSigning {
    internal class HttpRequestForSigning {
        public HttpMethod Method { get; set; }
        public Uri RequestUri { get; set; }
        public HttpRequestHeaders Headers { get; set; } = new HttpRequestMessage().Headers;
    }
}