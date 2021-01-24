using System.Collections.Generic;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    internal class ClientDataRecord {
        public string Id { get; set; }
        public string Name { get; set; }
        public string SigType { get; set; }
        public string SigParameter { get; set; }
        public string SigHashAlgorithm { get; set; }
        public bool IsSigParameterEncrypted { get; set; }
        public double? ClockSkew { get; set; }
        public string RequestTargetEscaping { get; set; }
        public double? NonceLifetime { get; set; }
        public IList<ClaimDataRecord> Claims { get; set; }
        public int? V { get; set; }

        public static int GetV() {
            return 1;
        }
    }
}