using System;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    public class CreationTimeVerificationTaskTests {
        private readonly CreationTimeVerificationTask _sut;
        private readonly ISystemClock _systemClock;

        public CreationTimeVerificationTaskTests() {
            FakeFactory.Create(out _systemClock);
            _sut = new CreationTimeVerificationTask(_systemClock);
        }

        public class Verify : CreationTimeVerificationTaskTests {
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
            public async Task WhenSignatureDoesNotSpecifyACreationTime_AndItIsRequired_ReturnsSignatureVerificationFailure() {
                _signature.Created = null;
                _signature.Headers = _signature.Headers.Concat(new[] {HeaderName.PredefinedHeaderNames.Created}).ToArray(); // It's required when it is part of the signature

                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().NotBeNull().And.BeAssignableTo<SignatureVerificationFailure>()
                    .Which.Code.Should().Be("INVALID_CREATED_HEADER");
            }
            
            [Fact]
            public async Task WhenSignatureDoesNotSpecifyACreationTime_AndItIsNotRequired_ReturnsNull() {
                _signature.Created = null;
                _signature.Headers = _signature.Headers.Where(h => h != HeaderName.PredefinedHeaderNames.Created).ToArray();
                
                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().BeNull();
            }

            [Fact]
            public async Task WhenSignatureCreationTimeIsInTheFuture_AndInsideClockSkew_ReturnsNull() {
                _signature.Created = _now.Add(_client.ClockSkew).AddSeconds(-1);
                _signature.Headers = _signature.Headers.Concat(new[] {HeaderName.PredefinedHeaderNames.Created}).ToArray();

                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().BeNull();
            }
            
            [Fact]
            public async Task WhenSignatureCreationTimeIsInTheFuture_AndEqualToClockSkew_ReturnsNull() {
                _signature.Created = _now.Add(_client.ClockSkew);
                _signature.Headers = _signature.Headers.Concat(new[] {HeaderName.PredefinedHeaderNames.Created}).ToArray();

                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().BeNull();
            }
            
            [Fact]
            public async Task WhenSignatureCreationTimeIsInTheFuture_AndOutsideClockSkew_ReturnsSignatureVerificationFailure() {
                _signature.Created = _now.Add(_client.ClockSkew).AddSeconds(1);
                _signature.Headers = _signature.Headers.Concat(new[] {HeaderName.PredefinedHeaderNames.Created}).ToArray();

                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().NotBeNull().And.BeAssignableTo<SignatureVerificationFailure>()
                    .Which.Code.Should().Be("INVALID_CREATED_HEADER");
            }

            [Fact]
            public async Task WhenSignatureCreationTimeIsNow_ReturnsNull() {
                _signature.Created = _now;
                _signature.Headers = _signature.Headers.Concat(new[] {HeaderName.PredefinedHeaderNames.Created}).ToArray();

                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().BeNull();
            }

            [Fact]
            public async Task WhenSignatureCreationTimeIsInThePast_ReturnsNull() {
                _signature.Created = _now.AddHours(-1);
                _signature.Headers = _signature.Headers.Concat(new[] {HeaderName.PredefinedHeaderNames.Created}).ToArray();

                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().BeNull();
            }
        }
    }
}