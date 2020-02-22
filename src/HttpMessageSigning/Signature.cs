using System;

namespace Dalion.HttpMessageSigning {
    public class Signature {
        public KeyId KeyId { get; set; }
        public Algorithm Algorithm { get; set; }
        public DateTimeOffset? Created { get; set; }
        public DateTimeOffset? Expires { get; set; }
        public Header[] Headers { get; set; } = Array.Empty<Header>();
    }
}