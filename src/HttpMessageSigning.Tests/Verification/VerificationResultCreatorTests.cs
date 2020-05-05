using System;
using System.Security.Claims;
using System.Security.Cryptography;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification {
    public class VerificationResultCreatorTests {
        private readonly Client _client;
        private readonly Signature _signature;
        private readonly IClaimsPrincipalFactory _claimsPrincipalFactory;
        private readonly VerificationResultCreator _sut;

        public VerificationResultCreatorTests() {
            FakeFactory.Create(out _claimsPrincipalFactory);
            _signature = new Signature {KeyId = "client1"};
            _client = new Client("client1", "Unit test app", new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA256), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
            _sut = new VerificationResultCreator(_client, _signature, _claimsPrincipalFactory);
        }

        public class CreateForSuccess : VerificationResultCreatorTests {
            [Fact]
            public void GivenNullClient_ThrowsInvalidOperationException() {
                var sut = new VerificationResultCreator(null, _signature, _claimsPrincipalFactory);
                Action act = () => sut.CreateForSuccess();
                act.Should().Throw<InvalidOperationException>();
            }

            [Fact]
            public void GivenNullSignature_ThrowsInvalidOperationException() {
                var sut = new VerificationResultCreator(_client, null, _claimsPrincipalFactory);
                Action act = () => sut.CreateForSuccess();
                act.Should().Throw<InvalidOperationException>();
            }

            [Fact]
            public void ReturnsSuccessInstanceWithExpectedPropertyValues() {
                var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] {new Claim("name", "john.doe")}));
                A.CallTo(() => _claimsPrincipalFactory.CreateForClient(_client))
                    .Returns(principal);
                
                var actual = _sut.CreateForSuccess();

                actual.Should().NotBeNull().And.BeAssignableTo<RequestSignatureVerificationResultSuccess>();
                var expected = new RequestSignatureVerificationResultSuccess(_client, _signature, principal);
                actual.As<RequestSignatureVerificationResultSuccess>().Should().BeEquivalentTo(expected);
            }
        }

        public class CreateForFailure : VerificationResultCreatorTests {
            private readonly HeaderMissingSignatureVerificationFailure _failure;

            public CreateForFailure() {
                _failure = SignatureVerificationFailure.HeaderMissing("You didn't say the magic word.");
            }

            [Fact]
            public void GivenNullFailure_ThrowsArgumentNullException() {
                Action act = () => _sut.CreateForFailure(null);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void AllowsForNullClient() {
                var sut = new VerificationResultCreator(null, _signature, _claimsPrincipalFactory);
                Action act = () => sut.CreateForFailure(_failure);
                act.Should().NotThrow();
            }

            [Fact]
            public void AllowsForNullSignature() {
                var sut = new VerificationResultCreator(_client, null, _claimsPrincipalFactory);
                Action act = () => sut.CreateForFailure(_failure);
                act.Should().NotThrow();
            }

            [Fact]
            public void ReturnsFailureInstanceWithExpectedPropertyValues() {
                var actual = _sut.CreateForFailure(_failure);
                
                actual.Should().NotBeNull().And.BeAssignableTo<RequestSignatureVerificationResultFailure>();
                var expected = new RequestSignatureVerificationResultFailure(_client, _signature, _failure);
                actual.As<RequestSignatureVerificationResultFailure>().Should().BeEquivalentTo(expected);
            }
        }
    }
}