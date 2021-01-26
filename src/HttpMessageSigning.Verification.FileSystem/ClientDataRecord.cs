using System;
using System.Linq;
using System.Xml.Linq;

namespace Dalion.HttpMessageSigning.Verification.FileSystem {
    internal class ClientDataRecord {
        public string Id { get; set; }
        public string Name { get; set; }
        public SignatureAlgorithmDataRecord SigAlg { get; set; }
        public ClaimDataRecord[] Claims { get; set; }
        public double? ClockSkew { get; set; }
        public string Escaping { get; set; }
        public int? V { get; set; }
        public double? NonceLifetime { get; set; }

        public static int GetV() {
            return 1;
        }
        
        public XContainer ToXml() {
            return new XElement(nameof(Client),
                new XElement(nameof(Id), Id),
                new XElement(nameof(Name), Name),
                new XElement(nameof(ClockSkew), ClockSkew),
                new XElement(nameof(NonceLifetime), NonceLifetime),
                new XElement(nameof(Escaping), Escaping),
                new XElement(nameof(V), V ?? GetV()),
                SigAlg.ToXml(),
                new XElement(nameof(Claims), Claims?.Select(c => c.ToXml()) ?? Enumerable.Empty<XElement>())
            );
        }

        public static ClientDataRecord FromXml(XContainer xml) {
            if (xml == null) throw new ArgumentNullException(nameof(xml));

            var clockSkew = double.TryParse(xml.Element(nameof(ClockSkew))?.Value, out var c) ? c : 0;
            if (clockSkew <= 0) clockSkew = ClientOptions.Default.ClockSkew.TotalSeconds;
            
            var nonceLifetime = double.TryParse(xml.Element(nameof(NonceLifetime))?.Value, out var n) ? n : 0;
            if (nonceLifetime <= 0) nonceLifetime = ClientOptions.Default.NonceLifetime.TotalSeconds;
            
            var version = int.TryParse(xml.Element(nameof(V))?.Value, out var v) ? v : GetV();
            if (version <= 0) version = GetV();

            var escaping = xml.Element(nameof(Escaping))?.Value;
            if (string.IsNullOrEmpty(escaping) || !Enum.IsDefined(typeof(RequestTargetEscaping), escaping)) {
                escaping = ClientOptions.Default.RequestTargetEscaping.ToString();
            }
            
            return new ClientDataRecord {
                Id = xml.Element(nameof(Id))?.Value,
                Name = xml.Element(nameof(Name))?.Value,
                ClockSkew = clockSkew,
                NonceLifetime = nonceLifetime,
                Escaping = escaping,
                V = version,
                Claims = xml.Element(nameof(Claims))?.Elements()?.Select(ClaimDataRecord.FromXml)?.ToArray() ?? Array.Empty<ClaimDataRecord>(),
                SigAlg = SignatureAlgorithmDataRecord.FromXml(xml.Element(nameof(SigAlg)))
            };
        }
    }
}