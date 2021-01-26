using System;
using System.Security.Cryptography;
using System.Xml.Linq;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.FileSystem.Serialization {
    public class ClientDataRecordSerializerTests {
        private readonly ClientDataRecordSerializer _sut;

        public ClientDataRecordSerializerTests() {
            _sut = new ClientDataRecordSerializer();
        }

        [Fact]
        public void CanRoundTrip() {
            var dataRecord = new ClientDataRecord {
                Id = "client001",
                Name = "Client One",
                V = ClientDataRecord.GetV(),
                Claims = new [] {
                    new ClaimDataRecord {
                        Type = "t1",
                        Value = "v1",
                        OriginalIss = "oi",
                        Iss = "i",
                        ValueType = "vt"
                    },
                    new ClaimDataRecord {
                        Type = "t2",
                        Value = "v2",
                        OriginalIss = "oi",
                        Iss = "i",
                        ValueType = "vt"
                    }
                },
                ClockSkew = 180,
                NonceLifetime = 120,
                Escaping = "RFC2396",
                SigAlg = new SignatureAlgorithmDataRecord {
                    Type = "HMAC",
                    Param = "<Secret>s3cr3t</Secret>",
                    Hash = HashAlgorithmName.MD5.Name,
                    Encrypted = false
                }
            };

            var xml = _sut.Serialize(dataRecord).ToString();

            var xContainer = XElement.Parse(xml);
            var deserialized = _sut.Deserialize(xContainer);
            
            deserialized.Should().BeEquivalentTo(dataRecord);
        }

        [Fact]
        public void FillsInValidDefaultsInMinimalObject() {
            var dataRecord = new ClientDataRecord {
                Id = "client001",
                SigAlg = new SignatureAlgorithmDataRecord {
                    Type = "HMAC",
                    Param = "<Secret>s3cr3t</Secret>",
                    Hash = HashAlgorithmName.MD5.Name,
                    Encrypted = false
                }
            };

            var xml = _sut.Serialize(dataRecord).ToString();

            var xContainer = XElement.Parse(xml);
            var deserialized = _sut.Deserialize(xContainer);
            
            var expected = new ClientDataRecord {
                Id = "client001",
                Name = "",
                V = 1,
                Claims = Array.Empty<ClaimDataRecord>(),
                ClockSkew = 60,
                NonceLifetime = 300,
                Escaping = "RFC3986",
                SigAlg = new SignatureAlgorithmDataRecord {
                    Type = "HMAC",
                    Param = "<Secret>s3cr3t</Secret>",
                    Hash = HashAlgorithmName.MD5.Name,
                    Encrypted = false
                }
            };
            deserialized.Should().BeEquivalentTo(expected);
        }
    }
}