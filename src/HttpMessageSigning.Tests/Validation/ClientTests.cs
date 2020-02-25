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
                Action act = () => new Client((KeyId)nullOrEmpty, (HMACSecret)"s3cr3t", HashAlgorithm.SHA256);
                act.Should().Throw<ArgumentException>();
            }

            [Fact]
            public void DoesNotAcceptNullSecrets() {
                Action act = () => new Client((KeyId)"id1", null, HashAlgorithm.SHA256);
                act.Should().Throw<ArgumentException>();
            }

            [Fact]
            public void DoesNotAcceptNoneHashAlgorithm() {
                Action act = () => new Client((KeyId)"id1", (HMACSecret)"s3cr3t", HashAlgorithm.None);
                act.Should().Throw<ArgumentException>();
            }
        }

        public class Equality : ClientTests {
            [Fact]
            public void WhenIdIsTheSame_AreEqual() {
                var first = new Client((KeyId)"id1", (HMACSecret)"s3cr3t", HashAlgorithm.SHA256);
                var second = new Client((KeyId)"id1", (HMACSecret)"s3cr3t", HashAlgorithm.SHA256);

                first.Equals(second).Should().BeTrue();
                first.GetHashCode().Should().Be(second.GetHashCode());
            }

            [Fact]
            public void WhenIdIsTheSame_AndTheOtherPropertiesAreDifferent_AreEqual() {
                var first = new Client((KeyId)"id1", (HMACSecret)"somethingElse", HashAlgorithm.SHA512);
                var second = new Client((KeyId)"id1", (HMACSecret)"s3cr3t", HashAlgorithm.SHA256);

                first.Equals(second).Should().BeTrue();
                first.GetHashCode().Should().Be(second.GetHashCode());
            }

            [Fact]
            public void WhenIdIsTheSame_ButDifferentlyCased_AreNotEqual() {
                var first = new Client((KeyId)"id1", (HMACSecret)"s3cr3t", HashAlgorithm.SHA256);
                var second = new Client((KeyId)"Id1", (HMACSecret)"s3cr3t", HashAlgorithm.SHA256);

                first.Equals(second).Should().BeFalse();
            }

            [Fact]
            public void IsNotEqualToANonKeyStoreEntry() {
                var first = new Client((KeyId)"id1", (HMACSecret)"s3cr3t", HashAlgorithm.SHA256);
                var second = new object();

                first.Equals(second).Should().BeFalse();
            }

            [Fact]
            public void SupportsInheritance() {
                var first = new Client((KeyId)"id1", (HMACSecret)"s3cr3t", HashAlgorithm.SHA256);
                var second = new InheritedClient((KeyId)"id1", (HMACSecret)"s3cr3t", HashAlgorithm.SHA256);

                first.Equals(second).Should().BeTrue();
                first.GetHashCode().Should().Be(second.GetHashCode());
            }

            public class InheritedClient : Client {
                public InheritedClient(KeyId id, Secret secret, HashAlgorithm hashAlgorithm) : base(id, secret, hashAlgorithm) { }
            }
        }

        public class ToStringRepresentation : ClientTests {
            private readonly Client _sut;

            public ToStringRepresentation() {
                _sut = new Client((KeyId)"id1", (HMACSecret)"s3cr3t", HashAlgorithm.SHA256);
            }

            [Fact]
            public void ReturnsId() {
                var actual = _sut.ToString();
                actual.Should().Be(_sut.Id);
            }
        }
    }
}