using Microsoft.AspNetCore.Http;

namespace Dalion.HttpMessageSigning.Verify {
    public class VerifyOptions {
        public HttpRequest Message { get; set; }
        public string Headers { get; set; }
        public string Created { get; set; }
        public string Expires { get; set; }
        public string PublicKey { get; set; }
        public string KeyId { get; set; }
        public string KeyType { get; set; }
        public string Algorithm { get; set; }
    }
}