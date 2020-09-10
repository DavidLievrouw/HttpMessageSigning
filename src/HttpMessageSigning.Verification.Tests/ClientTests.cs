using System;
using System.Security.Claims;
using System.Security.Cryptography;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification {
    public class ClientTests {
        public class Construction : ClientTests {
            [Fact]
            public void CreatesClient() {
                var actual =  new Client(
                    (KeyId)"c1", 
                    "Unit test app", 
                    new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256),
                    TimeSpan.FromMinutes(1), 
                    TimeSpan.FromMinutes(2),
                    RequestTargetEscaping.RFC2396,
                    new Claim("c1", "v1"),
                    new Claim("c2", "v2"));
                
                actual.Id.Should().Be((KeyId)"c1");
                actual.Name.Should().Be("Unit test app");
                actual.ClockSkew.Should().Be(TimeSpan.FromMinutes(2));
                actual.NonceLifetime.Should().Be(TimeSpan.FromMinutes(1));
                actual.Claims.Should().BeEquivalentTo(new Claim("c1", "v1"), new Claim("c2", "v2"));
                actual.RequestTargetEscaping.Should().Be(RequestTargetEscaping.RFC2396);
                actual.SignatureAlgorithm.Should().BeAssignableTo<HMACSignatureAlgorithm>();
            }
            
            [Fact]
            public void CreatesClientWithOptions() {
                var options = new ClientOptions {
                    Claims = new[] {
                        new Claim("c1", "v1"),
                        new Claim("c2", "v2")
                    },
                    ClockSkew = TimeSpan.FromMinutes(2),
                    NonceLifetime = TimeSpan.FromMinutes(1),
                    RequestTargetEscaping = RequestTargetEscaping.RFC2396
                };
                var actual = new Client(
                    (KeyId) "c1",
                    "Unit test app",
                    new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256),
                    options);
                
                actual.Id.Should().Be((KeyId)"c1");
                actual.Name.Should().Be("Unit test app");
                actual.ClockSkew.Should().Be(TimeSpan.FromMinutes(2));
                actual.NonceLifetime.Should().Be(TimeSpan.FromMinutes(1));
                actual.Claims.Should().BeEquivalentTo(new Claim("c1", "v1"), new Claim("c2", "v2"));
                actual.RequestTargetEscaping.Should().Be(RequestTargetEscaping.RFC2396);
                actual.SignatureAlgorithm.Should().BeAssignableTo<HMACSignatureAlgorithm>();
            }

            [Fact]
            public void AcceptsNullClaims() {
                var options = new ClientOptions {
                    Claims = null
                };
                var actual = new Client(
                    (KeyId) "c1",
                    "Unit test app",
                    new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256),
                    options);
                
                actual.Claims.Should().NotBeNull().And.BeEmpty();
            }
            
            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void DoesNotAcceptNullOrEmptyKeyIds(string nullOrEmpty) {
                Action act = () => new Client(
                    (KeyId)nullOrEmpty, 
                    "Unit test app", 
                    new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256),
                    TimeSpan.FromMinutes(1), 
                    TimeSpan.FromMinutes(1),
                    RequestTargetEscaping.RFC2396);
                act.Should().Throw<ArgumentException>();
            }
            
            [Theory]
            [InlineData(0)]
            [InlineData(-1)]
            [InlineData(-99)]
            public void DoesNotAcceptZeroOrNegativeNonceLifetime(int seconds) {
                Action act = () => new Client(
                    (KeyId)"id1",
                    "Unit test app", 
                    new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256),
                    TimeSpan.FromSeconds(seconds), 
                    TimeSpan.FromMinutes(1),
                    RequestTargetEscaping.RFC2396);
                act.Should().Throw<ValidationException>();
            }
            
            [Theory]
            [InlineData(0)]
            [InlineData(-1)]
            [InlineData(-99)]
            public void DoesNotAcceptZeroOrNegativeClockSkew(int seconds) {
                Action act = () => new Client(
                    (KeyId)"id1",
                    "Unit test app", 
                    new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256),
                    TimeSpan.FromMinutes(1), 
                    TimeSpan.FromSeconds(seconds),
                    RequestTargetEscaping.RFC2396);
                act.Should().Throw<ValidationException>();
            }
            
            [Fact]
            public void DoesNotAcceptUnknownRequestTargetEscaping() {
                var unsupported = (RequestTargetEscaping) (-99);
                Action act = () => new Client(
                    (KeyId)"id1",
                    "Unit test app", 
                    new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256),
                    TimeSpan.FromMinutes(1), 
                    TimeSpan.FromMinutes(2),
                    unsupported);
                act.Should().Throw<ValidationException>();
            }
            
            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void DoesNotAcceptNullOrEmptyName(string nullOrEmpty) {
                Action act = () => new Client(
                    (KeyId)"id1", 
                    nullOrEmpty, 
                    new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256),
                    TimeSpan.FromMinutes(1),
                    TimeSpan.FromMinutes(1),
                    RequestTargetEscaping.RFC2396);
                act.Should().Throw<ArgumentException>();
            }
            
            [Fact]
            public void DoesNotAcceptNullSignatureAlgorithm() {
                Action act = () => new Client(
                    (KeyId)"id1", 
                    "Unit test app",
                    null, 
                    TimeSpan.FromMinutes(1), 
                    TimeSpan.FromMinutes(1),
                    RequestTargetEscaping.RFC2396);
                act.Should().Throw<ArgumentException>();
            }
            
            [Theory]
            [InlineData(0)]
            [InlineData(-1)]
            [InlineData(-99)]
            public void DoesNotAcceptZeroOrEmptyNonceLifetime(int seconds) {
                Action act = () => new Client(
                    (KeyId)"id1", 
                    "Unit test app",
                    null, 
                    TimeSpan.FromSeconds(seconds), 
                    TimeSpan.FromMinutes(1),
                    RequestTargetEscaping.RFC2396);
                act.Should().Throw<ValidationException>();
            }

            [Fact]
            public void WhenUsingLegacyConstructor_SetsDefaultRequestTargetEscaping() {
                Client client = null;
#pragma warning disable 618
                Action act = () => client = new Client(
                    (KeyId)"id1", "Unit test app", 
                    new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256));
#pragma warning restore 618
                act.Should().NotThrow();
                client.RequestTargetEscaping.Should().Be(RequestTargetEscaping.RFC3986);
            }
            
            [Fact]
            public void WhenUsingLegacyConstructor_SetsDefaultNonceExpiration() {
                Client client = null;
#pragma warning disable 618
                Action act = () => client = new Client(
                    (KeyId)"id1", 
                    "Unit test app", 
                    new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256));
#pragma warning restore 618
                act.Should().NotThrow();
                client.NonceLifetime.Should().Be(ClientOptions.DefaultNonceLifetime);
            }
            
            [Fact]
            public void WhenUsingLegacyConstructor_SetsDefaultClockSkew() {
                Client client = null;
#pragma warning disable 618
                Action act = () => client = new Client(
                    (KeyId)"id1", 
                    "Unit test app", 
                    new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256));
#pragma warning restore 618
                act.Should().NotThrow();
                client.ClockSkew.Should().Be(ClientOptions.DefaultClockSkew);
            }
        }

        public class Equality : ClientTests {
            [Fact]
            public void IsNotEqualToNull() {
                var first = new Client(
                    (KeyId)"id1", 
                    "Unit test app", 
                    new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256),
                    TimeSpan.FromMinutes(1), 
                    TimeSpan.FromMinutes(1),
                    RequestTargetEscaping.RFC3986);
                Client second = null;

                // ReSharper disable once ExpressionIsAlwaysNull
                first.Equals(second).Should().BeFalse();
            }
            
            [Fact]
            public void IsEqualToSameReference() {
                var first = new Client(
                    (KeyId)"id1", 
                    "Unit test app", 
                    new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256), 
                    TimeSpan.FromMinutes(1),
                    TimeSpan.FromMinutes(1),
                    RequestTargetEscaping.RFC3986);
                
                first.Equals(first).Should().BeTrue();
                first.GetHashCode().Should().Be(first.GetHashCode());
            }
            
            [Fact]
            public void WhenIdIsTheSame_AreEqual() {
                var first = new Client(
                    (KeyId)"id1", 
                    "Unit test app",
                    new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256), 
                    TimeSpan.FromMinutes(1), 
                    TimeSpan.FromMinutes(1),
                    RequestTargetEscaping.RFC3986);
                var second = new Client(
                    (KeyId)"id1", 
                    "Unit test app", 
                    new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256), 
                    TimeSpan.FromMinutes(1), 
                    TimeSpan.FromMinutes(1),
                    RequestTargetEscaping.RFC3986);

                first.Equals(second).Should().BeTrue();
                first.GetHashCode().Should().Be(second.GetHashCode());
            }

            [Fact]
            public void WhenIdIsTheSame_AndTheOtherPropertiesAreDifferent_AreEqual() {
                var first = new Client(
                    (KeyId)"id1", 
                    "Unit test app", 
                    new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256),
                    TimeSpan.FromMinutes(1), 
                    TimeSpan.FromMinutes(1),
                    RequestTargetEscaping.RFC3986);
                var second = new Client(
                    (KeyId)"id1", 
                    "Unit test app",
                    new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA512), 
                    TimeSpan.FromMinutes(1), 
                    TimeSpan.FromMinutes(1),
                    RequestTargetEscaping.OriginalString,
                    new Claim("c1", "v1"));

                first.Equals(second).Should().BeTrue();
                first.GetHashCode().Should().Be(second.GetHashCode());
            }

            [Fact]
            public void WhenIdIsTheSame_ButDifferentlyCased_AreNotEqual() {
                var first = new Client(
                    (KeyId)"id1", 
                    "Unit test app", 
                    new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256),
                    TimeSpan.FromMinutes(1), 
                    TimeSpan.FromMinutes(1),
                    RequestTargetEscaping.RFC3986);
                var second = new Client(
                    (KeyId)"Id1",
                    "Unit test app", 
                    new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256),
                    TimeSpan.FromMinutes(1),
                    TimeSpan.FromMinutes(1),
                    RequestTargetEscaping.RFC3986);

                first.Equals(second).Should().BeFalse();
            }

            [Fact]
            public void IsNotEqualToANonKeyStoreEntry() {
                var first = new Client(
                    (KeyId)"id1", 
                    "Unit test app", 
                    new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256),
                    TimeSpan.FromMinutes(1), 
                    TimeSpan.FromMinutes(1),
                    RequestTargetEscaping.RFC3986);
                var second = new object();

                first.Equals(second).Should().BeFalse();
            }

            [Fact]
            public void SupportsInheritance() {
                var first = new Client(
                    (KeyId)"id1", 
                    "Unit test app", 
                    new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256), 
                    TimeSpan.FromMinutes(1), 
                    TimeSpan.FromMinutes(1),
                    RequestTargetEscaping.RFC3986);
                var second = new InheritedClient(
                    (KeyId)"id1",
                    "Unit test app", 
                    new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256), 
                    TimeSpan.FromMinutes(1), 
                    TimeSpan.FromMinutes(1),
                    RequestTargetEscaping.RFC3986);

                first.Equals(second).Should().BeTrue();
                first.GetHashCode().Should().Be(second.GetHashCode());
            }

            public class InheritedClient : Client {
                public InheritedClient(KeyId id, string name, ISignatureAlgorithm signatureAlgorithm, TimeSpan nonceLifetime, TimeSpan clockSkew,
                    RequestTargetEscaping requestTargetEscaping) : base(id, name, signatureAlgorithm, nonceLifetime, clockSkew, requestTargetEscaping) { }
            }
        }
        
        public class ToStringRepresentation : ClientTests {
            private readonly Client _sut;

            public ToStringRepresentation() {
                _sut = new Client(
                    (KeyId)"id1", 
                    "Unit test app", 
                    new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256), 
                    TimeSpan.FromMinutes(1),
                    TimeSpan.FromMinutes(1),
                    RequestTargetEscaping.RFC3986);
            }

            [Fact]
            public void ReturnsId() {
                var actual = _sut.ToString();
                actual.Should().Be(_sut.Id);
            }
        }
    }
}