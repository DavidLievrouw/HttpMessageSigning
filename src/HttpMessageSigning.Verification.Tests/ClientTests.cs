using System;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification {
    public class ClientTests {
        public class Construction : ClientTests {
            [Fact]
            public void CreatesClient() {
                var actual = new Client(
                    (KeyId) "c1",
                    "Unit test app",
                    new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256),
                    TimeSpan.FromMinutes(1),
                    TimeSpan.FromMinutes(2),
                    RequestTargetEscaping.RFC2396,
                    new Claim("c1", "v1"),
                    new Claim("c2", "v2"));

                actual.Id.Should().Be((KeyId) "c1");
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

                actual.Id.Should().Be((KeyId) "c1");
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
                    (KeyId) nullOrEmpty,
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
                    (KeyId) "id1",
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
                    (KeyId) "id1",
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
                    (KeyId) "id1",
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
                    (KeyId) "id1",
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
                    (KeyId) "id1",
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
                    (KeyId) "id1",
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
                    (KeyId) "id1", "Unit test app",
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
                    (KeyId) "id1",
                    "Unit test app",
                    new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256));
#pragma warning restore 618
                act.Should().NotThrow();
                client.NonceLifetime.Should().Be(ClientOptions.Default.NonceLifetime);
            }

            [Fact]
            public void WhenUsingLegacyConstructor_SetsDefaultClockSkew() {
                Client client = null;
#pragma warning disable 618
                Action act = () => client = new Client(
                    (KeyId) "id1",
                    "Unit test app",
                    new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256));
#pragma warning restore 618
                act.Should().NotThrow();
                client.ClockSkew.Should().Be(ClientOptions.Default.ClockSkew);
            }
        }

        public class CreateWithSignatureAlgorithm : ClientTests {
            private readonly HMACSignatureAlgorithm _signatureAlgorithm;
            private readonly Action<ClientOptions> _configureOptions;
            private readonly KeyId _keyId;
            private readonly string _name;

            public CreateWithSignatureAlgorithm() {
                _keyId = (KeyId) "id1";
                _name = "test client";
                _signatureAlgorithm = new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256);
                _configureOptions = opts => {
                    opts.Claims = new[] {new Claim("c1", "v1")};
                    opts.ClockSkew = TimeSpan.FromMinutes(10);
                    opts.NonceLifetime = TimeSpan.FromMinutes(3);
                    opts.RequestTargetEscaping = RequestTargetEscaping.OriginalString;
                };
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void DoesNotAcceptNullOrEmptyKeyIds(string nullOrEmpty) {
                Action act = () => Client.Create(nullOrEmpty, _name, _signatureAlgorithm, _configureOptions);
                act.Should().Throw<ArgumentException>();
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void DoesNotAcceptNullOrEmptyName(string nullOrEmpty) {
                Action act = () => Client.Create(_keyId, nullOrEmpty, _signatureAlgorithm, _configureOptions);
                act.Should().Throw<ArgumentException>();
            }

            [Fact]
            public void DoesNotAcceptNullSignatureAlgorithm() {
                Action act = () => Client.Create(_keyId, _name, signatureAlgorithm: null, _configureOptions);
                act.Should().Throw<ArgumentException>();
            }

            [Fact]
            public void DoesNotAcceptInvalidOptions() {
                Action<ClientOptions> invalidConfigureOptions = opts => {
                    opts.ClockSkew = TimeSpan.FromMinutes(-1); // Make invalid
                };
                Action act = () => Client.Create(_keyId, _name, _signatureAlgorithm, invalidConfigureOptions);
                act.Should().Throw<ValidationException>();
            }

            [Fact]
            public void AcceptsNullConfigureOptionsMethod() {
                Action act = () => Client.Create(_keyId, _name, _signatureAlgorithm, configure: null);
                act.Should().NotThrow();
            }

            [Fact]
            public void CallsConfigureOptionsMethodAndAppliesToClient() {
                var client = Client.Create(_keyId, _name, _signatureAlgorithm, _configureOptions);

                client.Id.Should().Be(_keyId);
                client.Name.Should().Be(_name);
                client.SignatureAlgorithm.Should().Be(_signatureAlgorithm);
                client.Claims.Should().BeEquivalentTo(new Claim("c1", "v1"));
                client.ClockSkew.Should().Be(TimeSpan.FromMinutes(10));
                client.NonceLifetime.Should().Be(TimeSpan.FromMinutes(3));
                client.RequestTargetEscaping.Should().Be(RequestTargetEscaping.OriginalString);
            }
        }

        public class CreateWithHMACSecret : ClientTests {
            private readonly string _secret;
            private readonly HashAlgorithmName _hashAlgorithm;
            private readonly Action<ClientOptions> _configureOptions;
            private readonly KeyId _keyId;
            private readonly string _name;

            public CreateWithHMACSecret() {
                _keyId = (KeyId) "id1";
                _name = "test client";
                _secret = "s3cr3t";
                _hashAlgorithm = HashAlgorithmName.SHA384;
                _configureOptions = opts => {
                    opts.Claims = new[] {new Claim("c1", "v1")};
                    opts.ClockSkew = TimeSpan.FromMinutes(10);
                    opts.NonceLifetime = TimeSpan.FromMinutes(3);
                    opts.RequestTargetEscaping = RequestTargetEscaping.OriginalString;
                };
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void DoesNotAcceptNullOrEmptyKeyIds(string nullOrEmpty) {
                Action act = () => Client.Create(nullOrEmpty, _name, _secret, _hashAlgorithm, _configureOptions);
                act.Should().Throw<ArgumentException>();
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void DoesNotAcceptNullOrEmptyName(string nullOrEmpty) {
                Action act = () => Client.Create(_keyId, nullOrEmpty, _secret, _hashAlgorithm, _configureOptions);
                act.Should().Throw<ArgumentException>();
            }

            [Fact]
            public void DoesNotAcceptNullSecret() {
                Action act = () => Client.Create(_keyId, _name, hmacSecret: null, _hashAlgorithm, _configureOptions);
                act.Should().Throw<ArgumentException>();
            }

            [Fact]
            public void AcceptsEmptySecret() {
                Action act = () => Client.Create(_keyId, _name, string.Empty, _hashAlgorithm, _configureOptions);
                act.Should().NotThrow();
            }

            [Fact]
            public void DoesNotAcceptInvalidOptions() {
                Action<ClientOptions> invalidConfigureOptions = opts => {
                    opts.ClockSkew = TimeSpan.FromMinutes(-1); // Make invalid
                };
                Action act = () => Client.Create(_keyId, _name, _secret, _hashAlgorithm, invalidConfigureOptions);
                act.Should().Throw<ValidationException>();
            }

            [Fact]
            public void AcceptsNullConfigureOptionsMethod() {
                Action act = () => Client.Create(_keyId, _name, _secret, _hashAlgorithm, configure: null);
                act.Should().NotThrow();
            }

            [Fact]
            public void GivenDefaultHashAlgorithmName_SetsDefaultHashAlgorithm() {
                var client = Client.Create(_keyId, _name, _secret);

                client.SignatureAlgorithm.HashAlgorithm.Should().Be(HashAlgorithmName.SHA256);
            }

            [Fact]
            public void CallsConfigureOptionsMethodAndAppliesToClient() {
                var client = Client.Create(_keyId, _name, _secret, _hashAlgorithm, _configureOptions);

                client.Id.Should().Be(_keyId);
                client.Name.Should().Be(_name);
                client.SignatureAlgorithm.Should().BeAssignableTo<HMACSignatureAlgorithm>();
                client.SignatureAlgorithm.As<HMACSignatureAlgorithm>().Key.Should().Equal(Encoding.UTF8.GetBytes(_secret));
                client.SignatureAlgorithm.HashAlgorithm.Should().Be(_hashAlgorithm);
                client.Claims.Should().BeEquivalentTo(new Claim("c1", "v1"));
                client.ClockSkew.Should().Be(TimeSpan.FromMinutes(10));
                client.NonceLifetime.Should().Be(TimeSpan.FromMinutes(3));
                client.RequestTargetEscaping.Should().Be(RequestTargetEscaping.OriginalString);
            }
        }

        public class CreateWithRSAPublicParameters : ClientTests, IDisposable {
            private readonly RSAParameters _publicParameters;
            private readonly HashAlgorithmName _hashAlgorithm;
            private readonly Action<ClientOptions> _configureOptions;
            private readonly KeyId _keyId;
            private readonly string _name;
            private readonly RSACryptoServiceProvider _rsa;

            public CreateWithRSAPublicParameters() {
                _keyId = (KeyId) "id1";
                _name = "test client";
                _rsa = new RSACryptoServiceProvider();
                _publicParameters = _rsa.ExportParameters(includePrivateParameters: false);
                _hashAlgorithm = HashAlgorithmName.SHA384;
                _configureOptions = opts => {
                    opts.Claims = new[] {new Claim("c1", "v1")};
                    opts.ClockSkew = TimeSpan.FromMinutes(10);
                    opts.NonceLifetime = TimeSpan.FromMinutes(3);
                    opts.RequestTargetEscaping = RequestTargetEscaping.OriginalString;
                };
            }

            public void Dispose() {
                _rsa?.Dispose();
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void DoesNotAcceptNullOrEmptyKeyIds(string nullOrEmpty) {
                Action act = () => Client.Create(nullOrEmpty, _name, _publicParameters, _hashAlgorithm, _configureOptions);
                act.Should().Throw<ArgumentException>();
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void DoesNotAcceptNullOrEmptyName(string nullOrEmpty) {
                Action act = () => Client.Create(_keyId, nullOrEmpty, _publicParameters, _hashAlgorithm, _configureOptions);
                act.Should().Throw<ArgumentException>();
            }

            [Fact]
            public void DoesNotAcceptNullPublicParameters() {
                Action act = () => Client.Create(_keyId, _name, (RSAParameters) default, _hashAlgorithm, _configureOptions);
                act.Should().Throw<ArgumentException>();
            }

            [Fact]
            public void DoesNotAcceptInvalidOptions() {
                Action<ClientOptions> invalidConfigureOptions = opts => {
                    opts.ClockSkew = TimeSpan.FromMinutes(-1); // Make invalid
                };
                Action act = () => Client.Create(_keyId, _name, _publicParameters, _hashAlgorithm, invalidConfigureOptions);
                act.Should().Throw<ValidationException>();
            }

            [Fact]
            public void AcceptsNullConfigureOptionsMethod() {
                Action act = () => Client.Create(_keyId, _name, _publicParameters, _hashAlgorithm, configure: null);
                act.Should().NotThrow();
            }

            [Fact]
            public void GivenDefaultHashAlgorithmName_SetsDefaultHashAlgorithm() {
                var client = Client.Create(_keyId, _name, _publicParameters);

                client.SignatureAlgorithm.HashAlgorithm.Should().Be(HashAlgorithmName.SHA256);
            }

            [Fact]
            public void CallsConfigureOptionsMethodAndAppliesToClient() {
                var client = Client.Create(_keyId, _name, _publicParameters, _hashAlgorithm, _configureOptions);

                client.Id.Should().Be(_keyId);
                client.Name.Should().Be(_name);
                client.SignatureAlgorithm.Should().BeAssignableTo<RSASignatureAlgorithm>();
                client.SignatureAlgorithm.HashAlgorithm.Should().Be(_hashAlgorithm);
                client.Claims.Should().BeEquivalentTo(new Claim("c1", "v1"));
                client.ClockSkew.Should().Be(TimeSpan.FromMinutes(10));
                client.NonceLifetime.Should().Be(TimeSpan.FromMinutes(3));
                client.RequestTargetEscaping.Should().Be(RequestTargetEscaping.OriginalString);

                var actualPublicKey = client.SignatureAlgorithm.As<RSASignatureAlgorithm>().GetPublicKey();
                actualPublicKey.D.Should().BeEquivalentTo(_publicParameters.D);
                actualPublicKey.DP.Should().BeEquivalentTo(_publicParameters.DP);
                actualPublicKey.DQ.Should().BeEquivalentTo(_publicParameters.DQ);
                actualPublicKey.Exponent.Should().BeEquivalentTo(_publicParameters.Exponent);
                actualPublicKey.InverseQ.Should().BeEquivalentTo(_publicParameters.InverseQ);
                actualPublicKey.Modulus.Should().BeEquivalentTo(_publicParameters.Modulus);
                actualPublicKey.P.Should().BeEquivalentTo(_publicParameters.P);
                actualPublicKey.Q.Should().BeEquivalentTo(_publicParameters.Q);
            }
        }

        public class CreateWithECDsaPublicParameters : ClientTests, IDisposable {
            private readonly ECParameters _publicParameters;
            private readonly HashAlgorithmName _hashAlgorithm;
            private readonly Action<ClientOptions> _configureOptions;
            private readonly KeyId _keyId;
            private readonly string _name;
            private readonly ECDsa _ecdsa;

            public CreateWithECDsaPublicParameters() {
                _keyId = (KeyId) "id1";
                _name = "test client";
                _ecdsa = ECDsa.Create();
                _publicParameters = _ecdsa.ExportParameters(includePrivateParameters: false);
                _hashAlgorithm = HashAlgorithmName.SHA384;
                _configureOptions = opts => {
                    opts.Claims = new[] {new Claim("c1", "v1")};
                    opts.ClockSkew = TimeSpan.FromMinutes(10);
                    opts.NonceLifetime = TimeSpan.FromMinutes(3);
                    opts.RequestTargetEscaping = RequestTargetEscaping.OriginalString;
                };
            }

            public void Dispose() {
                _ecdsa?.Dispose();
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void DoesNotAcceptNullOrEmptyKeyIds(string nullOrEmpty) {
                Action act = () => Client.Create(nullOrEmpty, _name, _publicParameters, _hashAlgorithm, _configureOptions);
                act.Should().Throw<ArgumentException>();
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void DoesNotAcceptNullOrEmptyName(string nullOrEmpty) {
                Action act = () => Client.Create(_keyId, nullOrEmpty, _publicParameters, _hashAlgorithm, _configureOptions);
                act.Should().Throw<ArgumentException>();
            }

            [Fact]
            public void DoesNotAcceptNullPublicParameters() {
                Action act = () => Client.Create(_keyId, _name, (ECParameters) default, _hashAlgorithm, _configureOptions);
                act.Should().Throw<ArgumentException>();
            }

            [Fact]
            public void DoesNotAcceptInvalidOptions() {
                Action<ClientOptions> invalidConfigureOptions = opts => {
                    opts.ClockSkew = TimeSpan.FromMinutes(-1); // Make invalid
                };
                Action act = () => Client.Create(_keyId, _name, _publicParameters, _hashAlgorithm, invalidConfigureOptions);
                act.Should().Throw<ValidationException>();
            }

            [Fact]
            public void AcceptsNullConfigureOptionsMethod() {
                Action act = () => Client.Create(_keyId, _name, _publicParameters, _hashAlgorithm, configure: null);
                act.Should().NotThrow();
            }

            [Fact]
            public void GivenDefaultHashAlgorithmName_SetsDefaultHashAlgorithm() {
                var client = Client.Create(_keyId, _name, _publicParameters);

                client.SignatureAlgorithm.HashAlgorithm.Should().Be(HashAlgorithmName.SHA256);
            }

            [Fact]
            public void CallsConfigureOptionsMethodAndAppliesToClient() {
                var client = Client.Create(_keyId, _name, _publicParameters, _hashAlgorithm, _configureOptions);

                client.Id.Should().Be(_keyId);
                client.Name.Should().Be(_name);
                client.SignatureAlgorithm.Should().BeAssignableTo<ECDsaSignatureAlgorithm>();
                client.SignatureAlgorithm.HashAlgorithm.Should().Be(_hashAlgorithm);
                client.Claims.Should().BeEquivalentTo(new Claim("c1", "v1"));
                client.ClockSkew.Should().Be(TimeSpan.FromMinutes(10));
                client.NonceLifetime.Should().Be(TimeSpan.FromMinutes(3));
                client.RequestTargetEscaping.Should().Be(RequestTargetEscaping.OriginalString);

                var actualPublicKey = client.SignatureAlgorithm.As<ECDsaSignatureAlgorithm>().GetPublicKey();
                actualPublicKey.Curve.Oid.FriendlyName.Should().BeEquivalentTo(_publicParameters.Curve.Oid.FriendlyName);
                actualPublicKey.Q.X.Should().BeEquivalentTo(_publicParameters.Q.X);
                actualPublicKey.Q.Y.Should().BeEquivalentTo(_publicParameters.Q.Y);
            }
        }

        public class Equality : ClientTests {
            [Fact]
            public void IsNotEqualToNull() {
                var first = new Client(
                    (KeyId) "id1",
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
                    (KeyId) "id1",
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
                    (KeyId) "id1",
                    "Unit test app",
                    new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256),
                    TimeSpan.FromMinutes(1),
                    TimeSpan.FromMinutes(1),
                    RequestTargetEscaping.RFC3986);
                var second = new Client(
                    (KeyId) "id1",
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
                    (KeyId) "id1",
                    "Unit test app",
                    new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256),
                    TimeSpan.FromMinutes(1),
                    TimeSpan.FromMinutes(1),
                    RequestTargetEscaping.RFC3986);
                var second = new Client(
                    (KeyId) "id1",
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
                    (KeyId) "id1",
                    "Unit test app",
                    new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256),
                    TimeSpan.FromMinutes(1),
                    TimeSpan.FromMinutes(1),
                    RequestTargetEscaping.RFC3986);
                var second = new Client(
                    (KeyId) "Id1",
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
                    (KeyId) "id1",
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
                    (KeyId) "id1",
                    "Unit test app",
                    new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256),
                    TimeSpan.FromMinutes(1),
                    TimeSpan.FromMinutes(1),
                    RequestTargetEscaping.RFC3986);
                var second = new InheritedClient(
                    (KeyId) "id1",
                    "Unit test app",
                    new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256),
                    TimeSpan.FromMinutes(1),
                    TimeSpan.FromMinutes(1),
                    RequestTargetEscaping.RFC3986);

                first.Equals(second).Should().BeTrue();
                first.GetHashCode().Should().Be(second.GetHashCode());
            }

            public class InheritedClient : Client {
                public InheritedClient(
                    KeyId id,
                    string name,
                    ISignatureAlgorithm signatureAlgorithm,
                    TimeSpan nonceLifetime,
                    TimeSpan clockSkew,
                    RequestTargetEscaping requestTargetEscaping)
                    : base(id, name, signatureAlgorithm, nonceLifetime, clockSkew, requestTargetEscaping) { }
            }
        }

        public class ToStringRepresentation : ClientTests {
            private readonly Client _sut;

            public ToStringRepresentation() {
                _sut = new Client(
                    (KeyId) "id1",
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