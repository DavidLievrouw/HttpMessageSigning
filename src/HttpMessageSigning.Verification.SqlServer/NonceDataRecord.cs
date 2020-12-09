using System;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    internal class NonceDataRecord {
        public string Id { get; set; }
        public string ClientId { get; set; }
        public string Value { get; set; }
        public DateTime Expiration { get; set; }
    }
}