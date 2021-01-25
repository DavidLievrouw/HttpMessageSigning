using System;
using System.Security.Claims;
using System.Xml.Linq;

namespace Dalion.HttpMessageSigning.Verification.FileSystem {
    internal class ClaimDataRecord {
        public string Iss { get; set; }
        public string OriginalIss { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public string ValueType { get; set; }
        
        public Claim ToClaim() {
            return new Claim(Type, Value, ValueType, Iss, OriginalIss);
        }

        public static ClaimDataRecord FromClaim(Claim claim) {
            if (claim == null) throw new ArgumentNullException(nameof(claim));
            
            return new ClaimDataRecord {
                Iss = claim.Issuer,
                Type = claim.Type,
                Value = claim.Value,
                OriginalIss = claim.OriginalIssuer,
                ValueType = claim.ValueType
            };
        }        
        
        public XContainer ToXml() {
            return new XElement(nameof(Claim),
                new XElement(nameof(Type), Type),
                new XElement(nameof(Value), Value),
                new XElement(nameof(Iss), Iss),
                new XElement(nameof(OriginalIss), OriginalIss),
                new XElement(nameof(ValueType), ValueType)
            );
        }

        public static ClaimDataRecord FromXml(XContainer xml) {
            if (xml == null) throw new ArgumentNullException(nameof(xml));
            
            return new ClaimDataRecord {
                Iss = xml.Element(nameof(Iss))?.Value,
                Type = xml.Element(nameof(Type))?.Value,
                Value = xml.Element(nameof(Value))?.Value,
                OriginalIss = xml.Element(nameof(OriginalIss))?.Value,
                ValueType = xml.Element(nameof(ValueType))?.Value
            };
        }
    }
}