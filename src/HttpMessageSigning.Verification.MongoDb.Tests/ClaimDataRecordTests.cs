using System;
using System.Security.Claims;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    public class ClaimDataRecordTests {
        private readonly ClaimDataRecordV2 _sut;

        public ClaimDataRecordTests() {
            _sut = new ClaimDataRecordV2();
        }

        public class ToClaim : ClaimDataRecordTests {
            [Fact]
            public void GivenNullValue_ThrowsArgumentNullException() {
                _sut.Value = null;
                Action act = () => _sut.ToClaim();
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void GivenNullType_ThrowsArgumentNullException() {
                _sut.Type = null;
                Action act = () => _sut.ToClaim();
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void OtherPropertiesCanBeNull() {
                _sut.Type = "t1";
                _sut.Value = "v1";
                _sut.OriginalIssuer = null;
                _sut.Issuer = null;
                _sut.ValueType = null;

                Claim actual = null;
                Action act = () => actual = _sut.ToClaim();
                act.Should().NotThrow();

                var expected = new Claim("t1", "v1");
                actual.Should().BeEquivalentTo(expected);
            }

            [Fact]
            public void GivenCompleteObject_ReturnsExpectedClaim() {
                _sut.Type = "t1";
                _sut.Value = "v1";
                _sut.OriginalIssuer = "oi";
                _sut.Issuer = "i";
                _sut.ValueType = "vt";

                Claim actual = null;
                Action act = () => actual = _sut.ToClaim();
                act.Should().NotThrow();

                var expected = new Claim("t1", "v1", "vt", "i", "oi");
                actual.Should().BeEquivalentTo(expected);
            }
        }

        public class FromClaim : ClaimDataRecordTests {
            [Fact]
            public void GivenNullClaim_ThrowsArgumentNullException() {
                Action act = () => ClaimDataRecordV2.FromClaim(null);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void CopiesAllValues() {
                var minimalClaim = new Claim("t1", "v1", "vt", "i", "oi");

                ClaimDataRecordV2 actual = null;
                Action act = () => actual = ClaimDataRecordV2.FromClaim(minimalClaim);
                act.Should().NotThrow();

                var expected = new ClaimDataRecordV2 {
                    Type = "t1",
                    Value = "v1",
                    OriginalIssuer = "oi",
                    Issuer = "i",
                    ValueType = "vt"
                };
                actual.Should().BeEquivalentTo(expected);
            }

            [Fact]
            public void GivenMinimalClaim_DoesNotThrow() {
                var minimalClaim = new Claim("t1", "v1");

                ClaimDataRecordV2 actual = null;
                Action act = () => actual = ClaimDataRecordV2.FromClaim(minimalClaim);
                act.Should().NotThrow();

                var expected = new ClaimDataRecordV2 {
                    Type = "t1",
                    Value = "v1",
                    OriginalIssuer = "LOCAL AUTHORITY",
                    Issuer = "LOCAL AUTHORITY",
                    ValueType = "http://www.w3.org/2001/XMLSchema#string"
                };
                actual.Should().BeEquivalentTo(expected);
            }
        }
    }
}