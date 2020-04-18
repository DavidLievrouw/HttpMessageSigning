using System;
using System.Net.Http;

namespace Dalion.HttpMessageSigning {
    public class HttpRequestForSigning : ICloneable {
        public HttpMethod Method { get; set; } = HttpMethod.Get;
        public string RequestUri { get; set; }
        public HeaderDictionary Headers { get; set; } = new HeaderDictionary();
        public byte[] Body { get; set; }
        
        public object Clone() {
            return new HttpRequestForSigning {
                Method = Method,
                RequestUri = RequestUri,
                Body = (byte[]) Body?.Clone(),
                Headers = Headers == null ? null : new HeaderDictionary(Headers.ToDictionary())
            };
        }
    }
}