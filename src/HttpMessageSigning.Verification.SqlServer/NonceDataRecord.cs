using System;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    internal class NonceDataRecord {
        public string ClientId { get; set; }
        public string Value { get; set; }
        public DateTimeOffset Expiration { get; set; }
        public int? V { get; set; }
        
        public int GetV() {
            return 1;
        }
    }
}