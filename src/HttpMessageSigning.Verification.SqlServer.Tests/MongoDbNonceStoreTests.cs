using System;
using System.Threading.Tasks;
using FluentAssertions;
using MongoDB.Driver;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    public class MongoDbNonceStoreTests : MongoIntegrationTest, IDisposable {
        private readonly string _collectionName;
        private readonly MongoDbNonceStore _sut;
        private readonly DateTimeOffset _now;
        private readonly MongoDatabaseClientProvider _mongoDatabaseClientProvider;

        public MongoDbNonceStoreTests(MongoSetup mongoSetup) : base(mongoSetup) {
            _collectionName = "nonces_" + Guid.NewGuid();
            _mongoDatabaseClientProvider = new MongoDatabaseClientProvider(Database);
            _sut = new MongoDbNonceStore(_mongoDatabaseClientProvider, _collectionName);
            
            _now = new DateTimeOffset(
                DateTimeOffset.UtcNow.Year, 
                DateTimeOffset.UtcNow.Month, 
                DateTimeOffset.UtcNow.Day, 
                DateTimeOffset.UtcNow.Hour, 
                DateTimeOffset.UtcNow.Minute, 
                DateTimeOffset.UtcNow.Second, 
                DateTimeOffset.UtcNow.Millisecond, 
                TimeSpan.Zero);
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
            public async Task CanHandleAlreadyExistingIndex() {
                var createIndexModel = new CreateIndexModel<NonceDataRecord>(
                    Builders<NonceDataRecord>.IndexKeys.Ascending(_ => _.Value),
                    new CreateIndexOptions {
                        Name = "idx_ttl",
                        ExpireAfter = TimeSpan.FromSeconds(5)
                    });
                var collection = _mongoDatabaseClientProvider.Provide().GetCollection<NonceDataRecord>(_collectionName);
                await collection.Indexes.CreateOneAsync(createIndexModel);
                
                var nonce = new Nonce(new KeyId("c1"), "abc123", _now.AddSeconds(30));

                await _sut.Register(nonce);

                var actual = await _sut.Get(nonce.ClientId, nonce.Value);

                actual.Should().BeEquivalentTo(nonce);
                actual.Expiration.Offset.Should().Be(TimeSpan.Zero);
            }
            
            [Fact]
            public async Task CanRoundTrip() {
                var nonce = new Nonce(new KeyId("c1"), "abc123", _now.AddSeconds(30));

                await _sut.Register(nonce);

                var actual = await _sut.Get(nonce.ClientId, nonce.Value);

                actual.Should().BeEquivalentTo(nonce);
                actual.Expiration.Offset.Should().Be(TimeSpan.Zero);
            }

            [Fact]
            public async Task Upserts() {
                var nonce1 = new Nonce(new KeyId("c1"), "abc123", _now.AddMinutes(1));
                await _sut.Register(nonce1);

                var nonce2 = new Nonce(nonce1.ClientId, nonce1.Value, _now.AddMinutes(2));
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

                var nonce1 = new Nonce(clientId, nonceValue, _now.AddMinutes(-1));
                await _sut.Register(nonce1);
                var nonce2 = new Nonce(clientId, nonceValue, _now.AddMinutes(1));
                await _sut.Register(nonce2);
                var nonce3 = new Nonce(clientId, nonceValue, _now.AddMinutes(2));
                await _sut.Register(nonce3);
                var nonce4 = new Nonce(clientId, nonceValue, _now.AddMinutes(1.5));
                await _sut.Register(nonce4);

                var actual = await _sut.Get(clientId, nonceValue);

                actual.Should().BeEquivalentTo(nonce4);
            }

            [Fact]
            public async Task CanGetAndDeserializeExistingNonce() {
                var nonce = new Nonce(new KeyId("c1"), "abc123", _now.AddMinutes(1));
                await _sut.Register(nonce);

                var actual = await _sut.Get(nonce.ClientId, nonce.Value);

                actual.Should().BeEquivalentTo(nonce);
            }
        }
    }
}