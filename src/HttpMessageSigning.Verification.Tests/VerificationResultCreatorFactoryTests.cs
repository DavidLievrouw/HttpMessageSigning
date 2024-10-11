using System;
using System.Security.Cryptography;
using Dalion.HttpMessageSigning.TestUtils;
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
            private readonly HttpRequestForVerification _requestForVerification;
            private readonly Client _client;

            public Create() {
                _requestForVerification = new HttpRequestForVerification {Signature = new Signature {KeyId = (KeyId)"client1"}};
                _client = new Client(
                    (KeyId)"client1", 
                    "Unit test app", 
                    new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256), 
                    TimeSpan.FromMinutes(1),
                    TimeSpan.FromMinutes(1),
                    RequestTargetEscaping.RFC3986);
            }

            [Fact]
            public void ClientCanBeNull() {
                Action act = () => _sut.Create(null, _requestForVerification);
                act.Should().NotThrow();
            }

            [Fact]
            public void RequestForVerificationCanBeNull() {
                Action act = () => _sut.Create(_client, null);
                act.Should().NotThrow();
            }

            [Fact]
            public void CreatesInstanceOfExpectedType() {
                var actual = _sut.Create(_client, _requestForVerification);
                actual.Should().NotBeNull().And.BeAssignableTo<VerificationResultCreator>();
            }
        }
    }
}