using System;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    internal class NonceDataRecord {
        public string ClientId { get; set; }
        public string Value { get; set; }
        public DateTimeOffset Expiration { get; set; }
    }
}