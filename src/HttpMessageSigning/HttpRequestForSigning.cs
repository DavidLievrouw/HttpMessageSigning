using System;
using System.Net.Http;

namespace Dalion.HttpMessageSigning {
    public class HttpRequestForSigning : ICloneable {
        public HttpMethod Method { get; set; } = HttpMethod.Get;
        public string RequestUri { get; set; }
        public HeaderDictionary Headers { get; set; } = new HeaderDictionary();
        
        public virtual object Clone() {
            return new HttpRequestForSigning {
                Method = Method,
                RequestUri = RequestUri,
                Headers = Headers == null ? null : new HeaderDictionary(Headers.ToDictionary())
            };
        }
    }
}