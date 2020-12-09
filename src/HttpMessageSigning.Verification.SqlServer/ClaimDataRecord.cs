using System;
using System.Security.Claims;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    internal class ClaimDataRecord {
        public string Issuer { get; set; }
        public string OriginalIssuer { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public string ValueType { get; set; }
        
        public Claim ToClaim() {
            return new Claim(Type, Value, ValueType, Issuer, OriginalIssuer);
        }

        public static ClaimDataRecord FromClaim(Claim claim) {
            if (claim == null) throw new ArgumentNullException(nameof(claim));
            
            return new ClaimDataRecord {
                Issuer = claim.Issuer,
                Type = claim.Type,
                Value = claim.Value,
                OriginalIssuer = claim.OriginalIssuer,
                ValueType = claim.ValueType
            };
        }
    }
}