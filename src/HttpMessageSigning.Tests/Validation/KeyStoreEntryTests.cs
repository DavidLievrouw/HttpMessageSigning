using System;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Validation {
    public class KeyStoreEntryTests {
        public class Construction : KeyStoreEntryTests {
            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void DoesNotAcceptNullOrEmptyIds(string nullOrEmpty) {
                Action act = () => new KeyStoreEntry(nullOrEmpty, "s3cr3t", SignatureAlgorithm.HMAC, HashAlgorithm.SHA256);
                act.Should().Throw<ArgumentException>();
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void DoesNotAcceptNullOrEmptySecrets(string nullOrEmpty) {
                Action act = () => new KeyStoreEntry("id1", nullOrEmpty, SignatureAlgorithm.HMAC, HashAlgorithm.SHA256);
                act.Should().Throw<ArgumentException>();
            }

            [Fact]
            public void DoesNotAcceptNoneHashAlgorithm() {
                Action act = () => new KeyStoreEntry("id1", "s3cr3t", SignatureAlgorithm.HMAC, HashAlgorithm.None);
                act.Should().Throw<ArgumentException>();
            }
        }

        public class Equality : KeyStoreEntryTests {
            [Fact]
            public void WhenIdIsTheSame_AreEqual() {
                var first = new KeyStoreEntry("id1", "s3cr3t", SignatureAlgorithm.HMAC, HashAlgorithm.SHA256);
                var second = new KeyStoreEntry("id1", "s3cr3t", SignatureAlgorithm.HMAC, HashAlgorithm.SHA256);

                first.Equals(second).Should().BeTrue();
                first.GetHashCode().Should().Be(second.GetHashCode());
            }

            [Fact]
            public void WhenIdIsTheSame_AndTheOtherPropertiesAreDifferent_AreEqual() {
                var first = new KeyStoreEntry("id1", "somethingElse", SignatureAlgorithm.RSA, HashAlgorithm.SHA512);
                var second = new KeyStoreEntry("id1", "s3cr3t", SignatureAlgorithm.HMAC, HashAlgorithm.SHA256);

                first.Equals(second).Should().BeTrue();
                first.GetHashCode().Should().Be(second.GetHashCode());
            }

            [Fact]
            public void WhenIdIsTheSame_ButDifferentlyCased_AreNotEqual() {
                var first = new KeyStoreEntry("id1", "s3cr3t", SignatureAlgorithm.HMAC, HashAlgorithm.SHA256);
                var second = new KeyStoreEntry("Id1", "s3cr3t", SignatureAlgorithm.HMAC, HashAlgorithm.SHA256);

                first.Equals(second).Should().BeFalse();
            }

            [Fact]
            public void IsNotEqualToANonKeyStoreEntry() {
                var first = new KeyStoreEntry("id1", "s3cr3t", SignatureAlgorithm.HMAC, HashAlgorithm.SHA256);
                var second = new object();

                first.Equals(second).Should().BeFalse();
            }

            [Fact]
            public void SupportsInheritance() {
                var first = new KeyStoreEntry("id1", "s3cr3t", SignatureAlgorithm.HMAC, HashAlgorithm.SHA256);
                var second = new InheritedKeyStoreEntry("id1", "s3cr3t", SignatureAlgorithm.HMAC, HashAlgorithm.SHA256);

                first.Equals(second).Should().BeTrue();
                first.GetHashCode().Should().Be(second.GetHashCode());
            }

            public class InheritedKeyStoreEntry : KeyStoreEntry {
                public InheritedKeyStoreEntry(string id, string secret, SignatureAlgorithm signatureAlgorithm, HashAlgorithm hashAlgorithm) : base(id, secret, signatureAlgorithm,
                    hashAlgorithm) { }
            }
        }

        public class ToStringRepresentation : KeyStoreEntryTests {
            private readonly KeyStoreEntry _sut;

            public ToStringRepresentation() {
                _sut = new KeyStoreEntry("id1", "s3cr3t", SignatureAlgorithm.HMAC, HashAlgorithm.SHA256);
            }

            [Fact]
            public void ReturnsId() {
                var actual = _sut.ToString();
                actual.Should().Be(_sut.Id);
            }
        }
    }
}