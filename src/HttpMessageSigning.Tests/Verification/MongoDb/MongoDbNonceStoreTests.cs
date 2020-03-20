using System;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    public class MongoDbNonceStoreTests : MongoIntegrationTest, IDisposable {
        private readonly string _collectionName;
        private readonly MongoDbNonceStore _sut;

        public MongoDbNonceStoreTests(MongoSetup mongoSetup) : base(mongoSetup) {
            _collectionName = "nonces";
            _sut = new MongoDbNonceStore(new MongoDatabaseClientProvider(Database), _collectionName);
        }

        public void Dispose() {
            _sut?.Dispose();
        }

        public class Register : MongoDbNonceStoreTests {
            public Register(MongoSetup mongoSetup) : base(mongoSetup) { }

            [Fact]
            public void GivenNullNonce_ThrowsArgumentNullException() {
                Func<Task> act = () => _sut.Register(null);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public async Task CanRoundTrip() {
                var nonce = new Nonce(new KeyId("c1"), "abc123", DateTimeOffset.UtcNow.AddMinutes(1));

                await _sut.Register(nonce);

                var actual = await _sut.Get(nonce.ClientId, nonce.Value);

                actual.Should().BeEquivalentTo(nonce);
            }

            [Fact]
            public async Task Upserts() {
                var nonce1 = new Nonce(new KeyId("c1"), "abc123", DateTimeOffset.UtcNow.AddMinutes(1));
                await _sut.Register(nonce1);

                var nonce2 = new Nonce(nonce1.ClientId, nonce1.Value, DateTimeOffset.UtcNow.AddMinutes(2));
                await _sut.Register(nonce2);

                var actual = await _sut.Get(nonce1.ClientId, nonce1.Value);

                actual.Should().BeEquivalentTo(nonce2);
            }
        }

        public class Get : MongoDbNonceStoreTests {
            public Get(MongoSetup mongoSetup) : base(mongoSetup) { }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptyId_ThrowsArgumentException(string nullOrEmpty) {
                Func<Task> act = () => _sut.Get(nullOrEmpty, "abc123");
                act.Should().Throw<ArgumentException>();
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptyNonceValue_ThrowsArgumentException(string nullOrEmpty) {
                Func<Task> act = () => _sut.Get(new KeyId("c1"), nullOrEmpty);
                act.Should().Throw<ArgumentException>();
            }

            [Fact]
            public async Task WhenNonceIsNotFound_ReturnsNull() {
                var actual = await _sut.Get("IDontExist", "abc123");
                actual.Should().BeNull();
            }

            [Fact]
            public async Task WhenMultipleNoncesAreRegistered_ReturnsTheLatestOne() {
                var clientId = new KeyId("c1");
                var nonceValue = "abc123";

                var nonce1 = new Nonce(clientId, nonceValue, DateTimeOffset.UtcNow.AddMinutes(-1));
                await _sut.Register(nonce1);
                var nonce2 = new Nonce(clientId, nonceValue, DateTimeOffset.UtcNow.AddMinutes(1));
                await _sut.Register(nonce2);
                var nonce3 = new Nonce(clientId, nonceValue, DateTimeOffset.UtcNow.AddMinutes(2));
                await _sut.Register(nonce3);
                var nonce4 = new Nonce(clientId, nonceValue, DateTimeOffset.UtcNow.AddMinutes(1.5));
                await _sut.Register(nonce4);

                var actual = await _sut.Get(clientId, nonceValue);

                actual.Should().BeEquivalentTo(nonce4);
            }

            [Fact]
            public async Task CanGetAndDeserializeExistingNonce() {
                var nonce = new Nonce(new KeyId("c1"), "abc123", DateTimeOffset.UtcNow.AddMinutes(1));
                await _sut.Register(nonce);

                var actual = await _sut.Get(nonce.ClientId, nonce.Value);

                actual.Should().BeEquivalentTo(nonce);
            }
        }
    }
}