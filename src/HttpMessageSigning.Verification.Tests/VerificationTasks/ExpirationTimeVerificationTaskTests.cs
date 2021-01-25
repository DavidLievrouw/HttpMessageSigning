using System;
using System.Linq;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.TestUtils;
using Dalion.HttpMessageSigning.Utils;
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
            private readonly HttpRequestForVerification _signedRequest;
            private readonly Client _client;
            private readonly Signature _signature;
            private readonly Func<HttpRequestForVerification, Signature, Client, Task<SignatureVerificationFailure>> _method;
            private readonly DateTimeOffset _now;

            public Verify() {
                _signature = (Signature) TestModels.Signature.Clone();
                _signedRequest = (HttpRequestForVerification) TestModels.RequestForVerification.Clone();
                _client = (Client) TestModels.Client.Clone();
                _method = (request, signature, client) => _sut.Verify(request, signature, client);

                _now = _signature.Created.Value.AddSeconds(3);
                A.CallTo(() => _systemClock.UtcNow).Returns(_now);
            }

            [Fact]
            public async Task WhenSignatureDoesNotSpecifyAExpirationTime_AndItRequired_ReturnsSignatureVerificationFailure() {
                _signature.Expires = null;
                _signature.Headers = _signature.Headers.Concat(new[] {HeaderName.PredefinedHeaderNames.Expires}).ToArray(); // It's required when it is part of the signature

                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().NotBeNull().And.BeAssignableTo<SignatureVerificationFailure>()
                    .Which.Code.Should().Be("HEADER_MISSING");
            }
            
            [Fact]
            public async Task WhenSignatureDoesNotSpecifyACreationTime_AndItIsNotRequired_ReturnsNull() {
                _signature.Expires = null;
                _signature.Headers = _signature.Headers.Where(h => h != HeaderName.PredefinedHeaderNames.Expires).ToArray();
                
                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().BeNull();
            }
            
            [Fact]
            public async Task WhenSignatureExpirationTimeIsInThePast_AndInsideClockSkew_ReturnsNull() {
                _signature.Expires = _now.Add(-_client.ClockSkew).AddSeconds(1);
                _signature.Headers = _signature.Headers.Concat(new[] {HeaderName.PredefinedHeaderNames.Expires}).ToArray();

                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().BeNull();
            }
            
            [Fact]
            public async Task WhenSignatureExpirationTimeIsInThePast_AndEqualToClockSkew_ReturnsNull() {
                _signature.Expires = _now.Add(-_client.ClockSkew);
                _signature.Headers = _signature.Headers.Concat(new[] {HeaderName.PredefinedHeaderNames.Expires}).ToArray();

                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().BeNull();
            }
            
            [Fact]
            public async Task WhenSignatureExpirationTimeIsInThePast_AndOutsideClockSkew_ReturnsSignatureVerificationFailure() {
                _signature.Expires = _now.Add(-_client.ClockSkew).AddSeconds(-1);
                _signature.Headers = _signature.Headers.Concat(new[] {HeaderName.PredefinedHeaderNames.Expires}).ToArray();

                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().NotBeNull().And.BeAssignableTo<SignatureVerificationFailure>()
                    .Which.Code.Should().Be("SIGNATURE_EXPIRED");
            }

            [Fact]
            public async Task WhenSignatureExpirationTimeIsNow_ReturnsNull() {
                _signature.Expires = _now;
                _signature.Headers = _signature.Headers.Concat(new[] {HeaderName.PredefinedHeaderNames.Expires}).ToArray();

                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().BeNull();
            }

            [Fact]
            public async Task WhenSignatureExpirationTimeIsInTheFuture_ReturnsNull() {
                _signature.Expires = _now.AddHours(1);
                _signature.Headers = _signature.Headers.Concat(new[] {HeaderName.PredefinedHeaderNames.Expires}).ToArray();

                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().BeNull();
            }
        }
    }
}