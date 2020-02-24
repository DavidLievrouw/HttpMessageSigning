using System;

namespace Dalion.HttpMessageSigning {
    public class Signature {
        public IKeyId KeyId { get; set; }
        public SignatureAlgorithm? SignatureAlgorithm { get; set; }
        public HashAlgorithm? HashAlgorithm { get; set; }
        public DateTimeOffset? Created { get; set; }
        public DateTimeOffset? Expires { get; set; }
        public HeaderName[] Headers { get; set; } = Array.Empty<HeaderName>();
        public string String { get; set; }
    }
}