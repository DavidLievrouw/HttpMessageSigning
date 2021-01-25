using System;
using System.Linq;
using System.Xml.Linq;

namespace Dalion.HttpMessageSigning.Verification.FileSystem {
    internal class SignatureAlgorithmDataRecord {
        public string Type { get; set; }
        public string Param { get; set; }
        public string Hash { get; set; }
        public bool Encrypted { get; set; }

        public XContainer ToXml() {
            return new XElement(nameof(ClientDataRecord.SigAlg),
                new XElement(nameof(Type), Type),
                new XElement(nameof(Hash), Hash),
                new XElement(nameof(Encrypted), Encrypted),
                new XElement(nameof(Param), XElement.Parse(Param))
            );
        }
        
        public static SignatureAlgorithmDataRecord FromXml(XContainer xml) {
            if (xml == null) throw new ArgumentNullException(nameof(xml));
            
            return new SignatureAlgorithmDataRecord {
                Type = xml.Element(nameof(Type))?.Value,
                Hash = xml.Element(nameof(Hash))?.Value,
                Encrypted = bool.TryParse(xml.Element(nameof(Encrypted))?.Value, out var e) && e,
                Param = xml.Element(nameof(Param))?.Elements()?.FirstOrDefault()?.ToString()
            };
        }
    }
}