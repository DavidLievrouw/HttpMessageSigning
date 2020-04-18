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
                Action act = () => new Client((KeyId)nullOrEmpty, "Unit test app", new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256), TimeSpan.FromMinutes(1));
                act.Should().Throw<ArgumentException>();
            }
            
            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void DoesNotAcceptNullOrEmptyNames(string nullOrEmpty) {
                Action act = () => new Client((KeyId)"id1", nullOrEmpty, new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256), TimeSpan.FromMinutes(1));
                act.Should().Throw<ArgumentException>();
            }
            
            [Fact]
            public void DoesNotAcceptNullSignatureAlgorithm() {
                Action act = () => new Client((KeyId)"id1", "Unit test app", null, TimeSpan.FromMinutes(1));
                act.Should().Throw<ArgumentException>();
            }
            
            [Theory]
            [InlineData(0)]
            [InlineData(-1)]
            [InlineData(-99)]
            public void DoesNotAcceptZeroOrEmptyNonceLifetime(int seconds) {
                Action act = () => new Client((KeyId)"id1", "Unit test app", null, TimeSpan.FromSeconds(seconds));
                act.Should().Throw<ArgumentOutOfRangeException>();
            }

            [Fact]
            public void WhenUsingLegacyConstructor_SetsDefaultNonceExpiration() {
                Client client = null;
#pragma warning disable 618
                Action act = () => client = new Client((KeyId)"id1", "Unit test app", new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256));
#pragma warning restore 618
                act.Should().NotThrow();
                client.NonceLifetime.Should().Be(Client.DefaultNonceLifetime);
            }
        }

        public class Equality : ClientTests {
            [Fact]
            public void IsNotEqualToNull() {
                var first = new Client((KeyId)"id1", "Unit test app", new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256), TimeSpan.FromMinutes(1));
                Client second = null;

                // ReSharper disable once ExpressionIsAlwaysNull
                first.Equals(second).Should().BeFalse();
            }
            
            [Fact]
            public void IsEqualToSameReference() {
                var first = new Client((KeyId)"id1", "Unit test app", new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256), TimeSpan.FromMinutes(1));
                
                first.Equals(first).Should().BeTrue();
                first.GetHashCode().Should().Be(first.GetHashCode());
            }
            
            [Fact]
            public void WhenIdIsTheSame_AreEqual() {
                var first = new Client((KeyId)"id1", "Unit test app", new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256), TimeSpan.FromMinutes(1));
                var second = new Client((KeyId)"id1", "Unit test app", new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256), TimeSpan.FromMinutes(1));

                first.Equals(second).Should().BeTrue();
                first.GetHashCode().Should().Be(second.GetHashCode());
            }

            [Fact]
            public void WhenIdIsTheSame_AndTheOtherPropertiesAreDifferent_AreEqual() {
                var first = new Client((KeyId)"id1", "Unit test app", new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256), TimeSpan.FromMinutes(1));
                var second = new Client((KeyId)"id1", "Unit test app", new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA512), TimeSpan.FromMinutes(1), new Claim("c1", "v1"));

                first.Equals(second).Should().BeTrue();
                first.GetHashCode().Should().Be(second.GetHashCode());
            }

            [Fact]
            public void WhenIdIsTheSame_ButDifferentlyCased_AreNotEqual() {
                var first = new Client((KeyId)"id1", "Unit test app", new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256), TimeSpan.FromMinutes(1));
                var second = new Client((KeyId)"Id1", "Unit test app", new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256), TimeSpan.FromMinutes(1));

                first.Equals(second).Should().BeFalse();
            }

            [Fact]
            public void IsNotEqualToANonKeyStoreEntry() {
                var first = new Client((KeyId)"id1", "Unit test app", new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256), TimeSpan.FromMinutes(1));
                var second = new object();

                first.Equals(second).Should().BeFalse();
            }

            [Fact]
            public void SupportsInheritance() {
                var first = new Client((KeyId)"id1", "Unit test app", new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256), TimeSpan.FromMinutes(1));
                var second = new InheritedClient((KeyId)"id1", "Unit test app", new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256), TimeSpan.FromMinutes(1));

                first.Equals(second).Should().BeTrue();
                first.GetHashCode().Should().Be(second.GetHashCode());
            }

            public class InheritedClient : Client {
                public InheritedClient(KeyId id, string name, ISignatureAlgorithm signatureAlgorithm, TimeSpan nonceLifetime) : base(id, name, signatureAlgorithm, TimeSpan.FromMinutes(1)) { }
            }
        }

        public class ToStringRepresentation : ClientTests {
            private readonly Client _sut;

            public ToStringRepresentation() {
                _sut = new Client((KeyId)"id1", "Unit test app", new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256), TimeSpan.FromMinutes(1));
            }

            [Fact]
            public void ReturnsId() {
                var actual = _sut.ToString();
                actual.Should().Be(_sut.Id);
            }
        }
    }
}