using System;
using System.Security.Cryptography;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification {
    public class VerificationResultCreatorFactoryTests {
        private readonly IClaimsPrincipalFactory _claimsPrincipalFactory;
        private readonly VerificationResultCreatorFactory _sut;

        public VerificationResultCreatorFactoryTests() {
            FakeFactory.Create(out _claimsPrincipalFactory);
            _sut = new VerificationResultCreatorFactory(_claimsPrincipalFactory);
        }

        public class Create : VerificationResultCreatorFactoryTests {
            private readonly Signature _signature;
            private readonly Client _client;

            public Create() {
                _signature = new Signature {KeyId = "client1"};
                _client = new Client("client1", "Unit test app", new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256), TimeSpan.FromMinutes(1));
            }

            [Fact]
            public void ClientCanBeNull() {
                Action act = () => _sut.Create(null, _signature);
                act.Should().NotThrow();
            }

            [Fact]
            public void SignatureCanBeNull() {
                Action act = () => _sut.Create(_client, null);
                act.Should().NotThrow();
            }

            [Fact]
            public void CreatesInstanceOfExpectedType() {
                var actual = _sut.Create(_client, _signature);
                actual.Should().NotBeNull().And.BeAssignableTo<VerificationResultCreator>();
            }
        }
    }
}