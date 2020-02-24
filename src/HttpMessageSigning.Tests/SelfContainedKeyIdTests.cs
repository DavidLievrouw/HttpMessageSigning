using System;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning {
    public class SelfContainedKeyIdTests {
        public class Construction : SelfContainedKeyIdTests {
            [Fact]
            public void GivenNullKey_SetsKeyToEmpty() {
                var actual = new SelfContainedKeyId(SignatureAlgorithm.RSA, HashAlgorithm.SHA1, null);
                actual.Value.Should().NotBeNull().And.BeEmpty();
            }
        }

        public class Equality : SelfContainedKeyIdTests {
            [Fact]
            public void WhenOtherIsNotAKeyId_AreNotEqual() {
                var first = new SelfContainedKeyId(SignatureAlgorithm.RSA, HashAlgorithm.SHA1, "abc123");
                var second = new object();

                first.Equals(second).Should().BeFalse();
            }

            [Fact]
            public void WhenHashAlgorithmIsDifferent_AreNotEqual() {
                var first = new SelfContainedKeyId(SignatureAlgorithm.RSA, HashAlgorithm.SHA1, "abc123");
                var second = new SelfContainedKeyId(SignatureAlgorithm.RSA, HashAlgorithm.SHA256, "abc123");

                first.Equals(second).Should().BeFalse();
                (first == second).Should().BeFalse();
                (first != second).Should().BeTrue();
            }

            [Fact]
            public void WhenSignatureAlgorithmIsDifferent_AreNotEqual() {
                var first = new SelfContainedKeyId(SignatureAlgorithm.HMAC, HashAlgorithm.SHA256, "abc123");
                var second = new SelfContainedKeyId(SignatureAlgorithm.RSA, HashAlgorithm.SHA256, "abc123");

                first.Equals(second).Should().BeFalse();
                (first == second).Should().BeFalse();
                (first != second).Should().BeTrue();
            }

            [Fact]
            public void WhenKeyIsDifferent_AreNotEqual() {
                var first = new SelfContainedKeyId(SignatureAlgorithm.HMAC, HashAlgorithm.SHA256, "abc123");
                var second = new SelfContainedKeyId(SignatureAlgorithm.HMAC, HashAlgorithm.SHA256, "xbc123");

                first.Equals(second).Should().BeFalse();
                (first == second).Should().BeFalse();
                (first != second).Should().BeTrue();
            }

            [Fact]
            public void WhenKeyIsDifferentlyCased_AreNotEqual() {
                var first = new SelfContainedKeyId(SignatureAlgorithm.HMAC, HashAlgorithm.SHA256, "abc123");
                var second = new SelfContainedKeyId(SignatureAlgorithm.HMAC, HashAlgorithm.SHA256, "Abc123");

                first.Equals(second).Should().BeFalse();
                (first == second).Should().BeFalse();
                (first != second).Should().BeTrue();
            }

            [Fact]
            public void WhenAllPropertiesAreEqual_AreEqual() {
                var first = new SelfContainedKeyId(SignatureAlgorithm.HMAC, HashAlgorithm.SHA256, "abc123");
                var second = new SelfContainedKeyId(SignatureAlgorithm.HMAC, HashAlgorithm.SHA256, "abc123");

                first.Equals(second).Should().BeTrue();
                (first == second).Should().BeTrue();
                (first != second).Should().BeFalse();
                first.GetHashCode().Should().Be(second.GetHashCode());
            }
        }

        public class ToStringConversion : SelfContainedKeyIdTests {
            [Fact]
            public void WhenKeyIsNull_ReturnsExpectedString() {
                var sut = new SelfContainedKeyId(SignatureAlgorithm.RSA, HashAlgorithm.SHA256, null);
                var actual = sut.ToString();
                actual.Should().Be("sig=rsa, hash=sha256, key=");
            }

            [Fact]
            public void ReturnsExpectedString() {
                var sut = new SelfContainedKeyId(SignatureAlgorithm.RSA, HashAlgorithm.SHA256, "Abc123");
                var actual = sut.ToString();
                actual.Should().Be("sig=rsa, hash=sha256, key=Abc123");
            }

            [Fact]
            public void CanRoundTrip() {
                var sut = new SelfContainedKeyId(SignatureAlgorithm.RSA, HashAlgorithm.SHA256, "Abc123");
                var str = sut.ToString();
                var actual = (SelfContainedKeyId) str;
                actual.Should().BeEquivalentTo(sut);
            }
        }

        public class FromStringConversion : SelfContainedKeyIdTests {
            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void WhenValueIsNullOrEmpty_ThrowsFormatException(string nullOrEmpty) {
                Action act = () => SelfContainedKeyId.Parse(nullOrEmpty);
                act.Should().Throw<FormatException>();
            }

            [Fact]
            public void WhenValueIsGibberish_ThrowsFormatException() {
                Action act = () => SelfContainedKeyId.Parse("{nonsense}");
                act.Should().Throw<FormatException>();
            }

            [Fact]
            public void WhenValueDoesNotHaveASignatureAlgorithm_ThrowsFormatException() {
                Action act = () => SelfContainedKeyId.Parse("hash=sha256, key=Abc123");
                act.Should().Throw<FormatException>();
            }

            [Fact]
            public void WhenValueDoesNotHaveASupportedSignatureAlgorithm_ThrowsFormatException() {
                Action act = () => SelfContainedKeyId.Parse("sig=blah, hash=sha256, key=Abc123");
                act.Should().Throw<FormatException>();
            }

            [Fact]
            public void WhenValueDoesNotHaveAHashAlgorithm_ThrowsFormatException() {
                Action act = () => SelfContainedKeyId.Parse("sig=rsa, key=Abc123");
                act.Should().Throw<FormatException>();
            }

            [Fact]
            public void WhenValueDoesNotHaveASupportedHashAlgorithm_ThrowsFormatException() {
                Action act = () => SelfContainedKeyId.Parse("sig=rsa, hash=blah, key=Abc123");
                act.Should().Throw<FormatException>();
            }

            [Fact]
            public void WhenValueDoesNotHaveAKey_ThrowsFormatException() {
                Action act = () => SelfContainedKeyId.Parse("sig=rsa, hash=sha256");
                act.Should().Throw<FormatException>();
            }

            [Fact]
            public void WhenKeyIsEmpty_ThrowsFormatException() {
                Action act = () => SelfContainedKeyId.Parse("sig=rsa, hash=sha256, key=");
                act.Should().Throw<FormatException>();
            }

            [Fact]
            public void WhenEverythingIsSpecifiedAndValid_ReturnsExpectedKeyId() {
                var parsed = new SelfContainedKeyId();
                Action act = () => parsed = SelfContainedKeyId.Parse("sig=rsa, hash=sha256, key=Abc123");
                act.Should().NotThrow();
                var expected = new SelfContainedKeyId(SignatureAlgorithm.RSA, HashAlgorithm.SHA256, "Abc123");
                parsed.Should().BeEquivalentTo(expected);
            }
        }
    }
}