using System;

namespace Dalion.HttpMessageSigning {
    public class Signature {
        public KeyId KeyId { get; set; }
        public Algorithm Algorithm { get; set; }
        public DateTimeOffset? Created { get; set; }
        public DateTimeOffset? Expires { get; set; }
        public HeaderName[] Headers { get; set; } = Array.Empty<HeaderName>();
        public string String { get; set; }
    }
}