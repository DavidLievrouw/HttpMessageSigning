using System.Net.Http;

namespace Dalion.HttpMessageSigning.SystemTests.Conformance.Canonicalize {
    public class CanonicalizeOptions {
        public HttpRequestMessage Message { get; set; }
        public string Headers { get; set; }
        public string Created { get; set; }
        public string Expires { get; set; }
    }
}