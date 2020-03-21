using System;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    public class ExpirationTimeVerificationTaskTests {
        private readonly ExpirationTimeVerificationTask _sut;
        private readonly ISystemClock _systemClock;

        public ExpirationTimeVerificationTaskTests() {
            FakeFactory.Create(out _systemClock);
            _sut = new ExpirationTimeVerificationTask(_systemClock);
        }

        public class Verify : ExpirationTimeVerificationTaskTests {
            private readonly HttpRequestForSigning _signedRequest;
            private readonly Client _client;
            private readonly Signature _signature;
            private readonly Func<HttpRequestForSigning, Signature, Client, Task<SignatureVerificationFailure>> _method;
            private readonly DateTimeOffset _now;

            public Verify() {
                _signature = (Signature) TestModels.Signature.Clone();
                _signedRequest = (HttpRequestForSigning) TestModels.Request.Clone();
                _client = (Client) TestModels.Client.Clone();
                _method = (request, signature, client) => _sut.Verify(request, signature, client);

                _now = _signature.Created.Value.AddSeconds(3);
                A.CallTo(() => _systemClock.UtcNow).Returns(_now);
            }

            [Fact]
            public async Task WhenSignatureDoesNotSpecifyAExpirationTime_ReturnsSignatureVerificationException() {
                _signature.Expires = null;

                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().NotBeNull().And.BeAssignableTo<SignatureVerificationFailure>()
                    .Which.Code.Should().Be("HEADER_MISSING");
            }

            [Fact]
            public async Task WhenSignatureExpirationTimeIsInThePast_ReturnsSignatureVerificationException() {
                _signature.Expires = _now.AddSeconds(-1);

                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().NotBeNull().And.BeAssignableTo<SignatureVerificationFailure>()
                    .Which.Code.Should().Be("SIGNATURE_EXPIRED");
            }

            [Fact]
            public async Task WhenSignatureExpirationTimeIsNow_ReturnsNull() {
                _signature.Expires = _now;

                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().BeNull();
            }

            [Fact]
            public async Task WhenSignatureExpirationTimeIsInTheFuture_ReturnsNull() {
                _signature.Expires = _now.AddSeconds(1);

                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().BeNull();
            }
        }
    }
}