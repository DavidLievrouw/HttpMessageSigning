using System;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    public class NonceVerificationTaskTests {
        private readonly INonceStore _nonceStore;
        private readonly ISystemClock _systemClock;
        private readonly NonceVerificationTask _sut;

        public NonceVerificationTaskTests() {
            FakeFactory.Create(out _nonceStore, out _systemClock);
            _sut = new NonceVerificationTask(_nonceStore, _systemClock);
        }

        public class Verify : NonceVerificationTaskTests {
            private readonly Client _client;
            private readonly Func<HttpRequestForSigning, Signature, Client, Task<SignatureVerificationFailure>> _method;
            private readonly Signature _signature;
            private readonly HttpRequestForSigning _signedRequest;
            private readonly DateTimeOffset _now;

            public Verify() {
                _signature = (Signature) TestModels.Signature.Clone();
                _signedRequest = (HttpRequestForSigning) TestModels.Request.Clone();
                _client = (Client) TestModels.Client.Clone();
                _method = (request, signature, client) => _sut.Verify(request, signature, client);
                
                _now = new DateTimeOffset(2020, 3, 20, 11, 52, 23, TimeSpan.Zero);
                A.CallTo(() => _systemClock.UtcNow).Returns(_now);
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public async Task WhenNonceIsMissing_ReturnsNull(string nullOrEmpty) {
                _signature.Nonce = nullOrEmpty;
                
                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().BeNull();
            }
            
            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public async Task WhenNonceIsMissing_DoesNotQueryNonceStore(string nullOrEmpty) {
                _signature.Nonce = nullOrEmpty;
                
                await _method(_signedRequest, _signature, _client);

                A.CallTo(() => _nonceStore.Get(A<KeyId>._, A<string>._))
                    .MustNotHaveHappened();
            }

            [Fact]
            public async Task WhenThereIsAPreviousNonce_ButItIsExpired_ReturnsNull() {
                var expiredNonce = new Nonce(_client.Id, _signature.Nonce, _now.AddSeconds(-1));
                A.CallTo(() => _nonceStore.Get(_client.Id, _signature.Nonce))
                    .Returns(expiredNonce);
                
                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().BeNull();
            }

            [Fact]
            public async Task WhenThereIsAPreviousNonce_ThatIsNotExpired_ReturnsSignatureVerificationFailure() {
                var nonce = new Nonce(_client.Id, _signature.Nonce, _now.AddSeconds(1));
                A.CallTo(() => _nonceStore.Get(_client.Id, _signature.Nonce))
                    .Returns(nonce);
                
                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().NotBeNull().And.BeAssignableTo<SignatureVerificationFailure>()
                    .Which.Code.Should().Be("INVALID_NONCE");
            }

            [Fact]
            public async Task WhenNonceIsVerified_RegistersNewNonceInTheStore() {
                A.CallTo(() => _nonceStore.Get(A<KeyId>._, A<string>._))
                    .Returns((Nonce) null);

                Nonce interceptedNonce = null;
                A.CallTo(() => _nonceStore.Register(A<Nonce>._))
                    .Invokes(call => interceptedNonce = call.GetArgument<Nonce>(0))
                    .Returns(Task.CompletedTask);
                    
                await _method(_signedRequest, _signature, _client);

                var expectedNonce = new Nonce(_client.Id, _signature.Nonce, _now.Add(_client.NonceLifetime));
                interceptedNonce.Should().BeEquivalentTo(expectedNonce);
            }

            [Fact]
            public async Task WhenNonceIsVerified_ReturnsNull() {
                A.CallTo(() => _nonceStore.Get(A<KeyId>._, A<string>._))
                    .Returns((Nonce) null);
                
                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().BeNull();
            }
        }
    }
}