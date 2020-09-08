using System;
using System.Net.Http;

namespace Dalion.HttpMessageSigning.SigningString {
    internal class HttpRequestForSignatureString {
        public HttpMethod Method { get; set; } = HttpMethod.Get;
        public Uri RequestUri { get; set; }
        public HeaderDictionary Headers { get; set; } = new HeaderDictionary();
    }
}