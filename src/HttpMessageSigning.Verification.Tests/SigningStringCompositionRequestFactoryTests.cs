using System;
using Dalion.HttpMessageSigning.Verification.VerificationTasks;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification {
    public class SigningStringCompositionRequestFactoryTests {
        private readonly SigningStringCompositionRequestFactory _sut;

        public SigningStringCompositionRequestFactoryTests() {
            _sut = new SigningStringCompositionRequestFactory();
        }

        public class CreateForVerification : SigningStringCompositionRequestFactoryTests {
            private readonly HttpRequestForVerification _signedRequest;
            private readonly Signature _signature;
            private readonly Client _client;

            public CreateForVerification() {
                _signedRequest = (HttpRequestForVerification) TestModels.RequestForVerification.Clone();
                _signature = (Signature) TestModels.Signature.Clone();
                _client = new Client(
                    TestModels.Client.Id, 
                    TestModels.Client.Name,
                    new CustomSignatureAlgorithm("TEST"), 
                    TestModels.Client.NonceLifetime,
                    TestModels.Client.ClockSkew,
                    TestModels.Client.Claims);
            }

            [Fact]
            public void GivenNullRequest_ThrowsArgumentNullException() {
                Action act = () => _sut.CreateForVerification(null, _client, _signature);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void GivenNullClient_ThrowsArgumentNullException() {
                Action act = () => _sut.CreateForVerification(_signedRequest, null, _signature);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void GivenNullSignature_ThrowsArgumentNullException() {
                Action act = () => _sut.CreateForVerification(_signedRequest, _client, null);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void CreatesRequestWithExpectedPropertyValues() {
                var actual = _sut.CreateForVerification(_signedRequest, _client, _signature);

                actual.Request.Should().Be(_signedRequest);
                actual.RequestTargetEscaping.Should().Be(RequestTargetEscaping.RFC3986); // ToDo #13
                actual.HeadersToInclude.Should().BeEquivalentTo(_signature.Headers, opts => opts.WithStrictOrdering());
                actual.TimeOfComposing.Should().Be(_signature.Created);
                actual.Nonce.Should().Be(_signature.Nonce);
            }

            [Fact]
            public void AcceptsNullNonceInSignature() {
                _signature.Nonce = null;
                
                var actual = _sut.CreateForVerification(_signedRequest, _client, _signature);

                actual.Nonce.Should().BeNull();
            }
            
            [Fact]
            public void CreatesRequestWithCalculatedExpiresValue() {
                var actual = _sut.CreateForVerification(_signedRequest, _client, _signature);

                var expectedExpires = _signature.Expires - _signature.Created;
                actual.Expires.Should().Be(expectedExpires);
            }
            
            [Fact]
            public void WhenSignatureDoesNotSpecifyACreationTime_UsesNullTimeOfComposing() {
                _signature.Created = null;
                
                var actual = _sut.CreateForVerification(_signedRequest, _client, _signature);

                actual.TimeOfComposing.Should().BeNull();
            }

            [Fact]
            public void WhenSignatureDoesNotSpecifyACreationTime_UsesNullExpiration() {
                _signature.Created = null;
                
                var actual = _sut.CreateForVerification(_signedRequest, _client, _signature);

                actual.Expires.Should().BeNull();
            }
            
            [Fact]
            public void WhenSignatureDoesNotSpecifyAnExpiration_StillSpecifiesTimeOfComposing() {
                _signature.Expires = null;
                
                var actual = _sut.CreateForVerification(_signedRequest, _client, _signature);

                actual.TimeOfComposing.Should().Be(_signature.Created);
            }

            [Fact]
            public void WhenSignatureDoesNotSpecifyAnExpiration_UsesNullExpiration() {
                _signature.Expires = null;
                
                var actual = _sut.CreateForVerification(_signedRequest, _client, _signature);

                actual.Expires.Should().BeNull();
            }
        }
    }
}