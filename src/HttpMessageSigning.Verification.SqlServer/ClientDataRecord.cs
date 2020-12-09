namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    internal class ClientDataRecord {
        public string Id { get; set; }

        public string Name { get; set; }
        public SignatureAlgorithmDataRecord SignatureAlgorithm { get; set; }
        public ClaimDataRecord[] Claims { get; set; }
        public double? ClockSkew { get; set; }
        public string RequestTargetEscaping { get; set; }
        public int? V { get; set; }
        public double? NonceLifetime { get; set; }
        
        public int GetV() {
            return 1;
        }
    }
}