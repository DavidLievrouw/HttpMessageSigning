using System.Net.Http;

namespace Dalion.HttpMessageSigning.Sign {
    public class SignOptions {
        public HttpRequestMessage Message { get; set; }
        public string Headers { get; set; }
        public string Created { get; set; }
        public string Expires { get; set; }
        public string PrivateKey { get; set; }
        public string KeyId { get; set; }
        public string KeyType { get; set; }
        public string Algorithm { get; set; }
    }
}