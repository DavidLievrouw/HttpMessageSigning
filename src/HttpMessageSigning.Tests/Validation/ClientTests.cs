using System;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Validation {
    public class ClientTests {
        public class Construction : ClientTests {
            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void DoesNotAcceptNullOrEmptyIds(string nullOrEmpty) {
                Action act = () => new Client(nullOrEmpty, "s3cr3t", SignatureAlgorithm.HMAC, HashAlgorithm.SHA256);
                act.Should().Throw<ArgumentException>();
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void DoesNotAcceptNullOrEmptySecrets(string nullOrEmpty) {
                Action act = () => new Client("id1", nullOrEmpty, SignatureAlgorithm.HMAC, HashAlgorithm.SHA256);
                act.Should().Throw<ArgumentException>();
            }

            [Fact]
            public void DoesNotAcceptNoneHashAlgorithm() {
                Action act = () => new Client("id1", "s3cr3t", SignatureAlgorithm.HMAC, HashAlgorithm.None);
                act.Should().Throw<ArgumentException>();
            }
        }

        public class Equality : ClientTests {
            [Fact]
            public void WhenIdIsTheSame_AreEqual() {
                var first = new Client("id1", "s3cr3t", SignatureAlgorithm.HMAC, HashAlgorithm.SHA256);
                var second = new Client("id1", "s3cr3t", SignatureAlgorithm.HMAC, HashAlgorithm.SHA256);

                first.Equals(second).Should().BeTrue();
                first.GetHashCode().Should().Be(second.GetHashCode());
            }

            [Fact]
            public void WhenIdIsTheSame_AndTheOtherPropertiesAreDifferent_AreEqual() {
                var first = new Client("id1", "somethingElse", SignatureAlgorithm.RSA, HashAlgorithm.SHA512);
                var second = new Client("id1", "s3cr3t", SignatureAlgorithm.HMAC, HashAlgorithm.SHA256);

                first.Equals(second).Should().BeTrue();
                first.GetHashCode().Should().Be(second.GetHashCode());
            }

            [Fact]
            public void WhenIdIsTheSame_ButDifferentlyCased_AreNotEqual() {
                var first = new Client("id1", "s3cr3t", SignatureAlgorithm.HMAC, HashAlgorithm.SHA256);
                var second = new Client("Id1", "s3cr3t", SignatureAlgorithm.HMAC, HashAlgorithm.SHA256);

                first.Equals(second).Should().BeFalse();
            }

            [Fact]
            public void IsNotEqualToANonKeyStoreEntry() {
                var first = new Client("id1", "s3cr3t", SignatureAlgorithm.HMAC, HashAlgorithm.SHA256);
                var second = new object();

                first.Equals(second).Should().BeFalse();
            }

            [Fact]
            public void SupportsInheritance() {
                var first = new Client("id1", "s3cr3t", SignatureAlgorithm.HMAC, HashAlgorithm.SHA256);
                var second = new InheritedClient("id1", "s3cr3t", SignatureAlgorithm.HMAC, HashAlgorithm.SHA256);

                first.Equals(second).Should().BeTrue();
                first.GetHashCode().Should().Be(second.GetHashCode());
            }

            public class InheritedClient : Client {
                public InheritedClient(string id, string secret, SignatureAlgorithm signatureAlgorithm, HashAlgorithm hashAlgorithm) : base(id, secret, signatureAlgorithm,
                    hashAlgorithm) { }
            }
        }

        public class ToStringRepresentation : ClientTests {
            private readonly Client _sut;

            public ToStringRepresentation() {
                _sut = new Client("id1", "s3cr3t", SignatureAlgorithm.HMAC, HashAlgorithm.SHA256);
            }

            [Fact]
            public void ReturnsId() {
                var actual = _sut.ToString();
                actual.Should().Be(_sut.Id);
            }
        }
    }
}