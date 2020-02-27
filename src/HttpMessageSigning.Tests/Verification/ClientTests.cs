using System;
using System.Security.Claims;
using System.Security.Cryptography;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification {
    public class ClientTests {
        public class Construction : ClientTests {
            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void DoesNotAcceptNullOrEmptyKeyIds(string nullOrEmpty) {
                Action act = () => new Client((KeyId)nullOrEmpty, new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256));
                act.Should().Throw<ArgumentException>();
            }
            
            [Fact]
            public void DoesNotAcceptNullSignatureAlgorithm() {
                Action act = () => new Client((KeyId)"id1", null);
                act.Should().Throw<ArgumentException>();
            }
        }

        public class Equality : ClientTests {
            [Fact]
            public void WhenIdIsTheSame_AreEqual() {
                var first = new Client((KeyId)"id1", new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256));
                var second = new Client((KeyId)"id1", new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256));

                first.Equals(second).Should().BeTrue();
                first.GetHashCode().Should().Be(second.GetHashCode());
            }

            [Fact]
            public void WhenIdIsTheSame_AndTheOtherPropertiesAreDifferent_AreEqual() {
                var first = new Client((KeyId)"id1", new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256));
                var second = new Client((KeyId)"id1", new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA512), new Claim("c1", "v1"));

                first.Equals(second).Should().BeTrue();
                first.GetHashCode().Should().Be(second.GetHashCode());
            }

            [Fact]
            public void WhenIdIsTheSame_ButDifferentlyCased_AreNotEqual() {
                var first = new Client((KeyId)"id1", new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256));
                var second = new Client((KeyId)"Id1", new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256));

                first.Equals(second).Should().BeFalse();
            }

            [Fact]
            public void IsNotEqualToANonKeyStoreEntry() {
                var first = new Client((KeyId)"id1", new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256));
                var second = new object();

                first.Equals(second).Should().BeFalse();
            }

            [Fact]
            public void SupportsInheritance() {
                var first = new Client((KeyId)"id1", new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256));
                var second = new InheritedClient((KeyId)"id1", new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256));

                first.Equals(second).Should().BeTrue();
                first.GetHashCode().Should().Be(second.GetHashCode());
            }

            public class InheritedClient : Client {
                public InheritedClient(KeyId id, ISignatureAlgorithm signatureAlgorithm) : base(id, signatureAlgorithm) { }
            }
        }

        public class ToStringRepresentation : ClientTests {
            private readonly Client _sut;

            public ToStringRepresentation() {
                _sut = new Client((KeyId)"id1", new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256));
            }

            [Fact]
            public void ReturnsId() {
                var actual = _sut.ToString();
                actual.Should().Be(_sut.Id);
            }
        }
    }
}