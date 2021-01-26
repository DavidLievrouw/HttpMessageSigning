using System;
using System.Xml.Linq;

namespace Dalion.HttpMessageSigning.Verification.FileSystem {
    internal class NonceDataRecord {
        public string ClientId { get; set; }
        public string Value { get; set; }
        public DateTimeOffset Expiration { get; set; }

        public XContainer ToXml() {
            return new XElement(nameof(Nonce),
                new XElement(nameof(ClientId), ClientId),
                new XElement(nameof(Value), Value),
                new XElement(nameof(Expiration), Expiration)
            );
        }

        public static NonceDataRecord FromXml(XContainer xml) {
            var expiration = DateTimeOffset.TryParse(xml.Element(nameof(Expiration))?.Value, out var e) ? e : DateTime.MinValue;

            return new NonceDataRecord {
                ClientId = xml.Element(nameof(ClientId))?.Value,
                Value = xml.Element(nameof(Value))?.Value,
                Expiration = expiration
            };
        }
    }
}