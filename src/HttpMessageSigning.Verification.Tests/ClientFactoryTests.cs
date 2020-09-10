using System;
using System.Security.Claims;
using System.Security.Cryptography;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification {
    public class ClientFactoryTests {
        private readonly HMACSignatureAlgorithm _signatureAlgorithm;
        private readonly Action<ClientOptions> _configureOptions;
        private readonly KeyId _keyId;
        private readonly string _name;

        public ClientFactoryTests() {
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
            Action act = () => ClientFactory.Create(nullOrEmpty, _name, _signatureAlgorithm, _configureOptions);
            act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void DoesNotAcceptNullOrEmptyName(string nullOrEmpty) {
            Action act = () => ClientFactory.Create(_keyId, nullOrEmpty, _signatureAlgorithm, _configureOptions);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void DoesNotAcceptNullSignatureAlgorithm() {
            Action act = () => ClientFactory.Create(_keyId, _name, signatureAlgorithm: null, _configureOptions);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void DoesNotAcceptInvalidOptions() {
            Action<ClientOptions> invalidConfigureOptions = opts => {
                opts.ClockSkew = TimeSpan.FromMinutes(-1); // Make invalid
            };
            Action act = () => ClientFactory.Create(_keyId, _name, _signatureAlgorithm, invalidConfigureOptions);
            act.Should().Throw<ValidationException>();
        }

        [Fact]
        public void AcceptsNullConfigureOptionsMethod() {
            Action act = () => ClientFactory.Create(_keyId, _name, _signatureAlgorithm, configure: null);
            act.Should().NotThrow();
        }

        [Fact]
        public void CallsConfigureOptionsMethodAndAppliesToClient() {
            var client = ClientFactory.Create(_keyId, _name, _signatureAlgorithm, _configureOptions);

            client.Id.Should().Be(_keyId);
            client.Name.Should().Be(_name);
            client.SignatureAlgorithm.Should().Be(_signatureAlgorithm);
            client.Claims.Should().BeEquivalentTo(new Claim("c1", "v1"));
            client.ClockSkew.Should().Be(TimeSpan.FromMinutes(10));
            client.NonceLifetime.Should().Be(TimeSpan.FromMinutes(3));
            client.RequestTargetEscaping.Should().Be(RequestTargetEscaping.OriginalString);
        }
    }
}