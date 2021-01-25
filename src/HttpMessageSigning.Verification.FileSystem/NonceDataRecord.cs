using System;

namespace Dalion.HttpMessageSigning.Verification.FileSystem {
    internal class NonceDataRecord {
        public string Id { get; set; }
        public string ClientId { get; set; }
        public string Value { get; set; }
        public DateTime Expiration { get; set; }
    }
}