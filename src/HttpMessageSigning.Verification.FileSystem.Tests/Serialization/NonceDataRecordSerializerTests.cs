using System;
using System.Xml.Linq;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.FileSystem.Serialization {
    public class NonceDataRecordSerializerTests {
        private readonly NonceDataRecordSerializer _sut;

        public NonceDataRecordSerializerTests() {
            _sut = new NonceDataRecordSerializer();
        }

        [Fact]
        public void CanRoundTrip() {
            var dataRecord = new NonceDataRecord {
                ClientId = "client001",
                Value = "abc",
                Expiration = new DateTime(2021, 1, 26, 15, 39, 22, DateTimeKind.Utc)
            };

            var xml = _sut.Serialize(dataRecord).ToString();

            var xContainer = XElement.Parse(xml);
            var deserialized = _sut.Deserialize(xContainer);

            deserialized.Should().BeEquivalentTo(dataRecord);
        }
    }
}