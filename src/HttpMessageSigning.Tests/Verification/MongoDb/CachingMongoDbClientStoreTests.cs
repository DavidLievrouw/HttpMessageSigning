using System;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    public class CachingMongoDbClientStoreTests : IDisposable {
        private readonly FakeMemoryCache _cache;
        private readonly IMongoDbClientStore _decorated;
        private readonly TimeSpan _expiration;
        private readonly IBackgroundTaskStarter _backgroundTaskStarter;
        private readonly CachingMongoDbClientStore _sut;

        public CachingMongoDbClientStoreTests() {
            FakeFactory.Create(out _decorated, out _backgroundTaskStarter);
            _cache = new FakeMemoryCache();
            _expiration = TimeSpan.FromSeconds(30);
            _sut = new CachingMongoDbClientStore(_decorated, _cache, _expiration, _backgroundTaskStarter);
        }

        public void Dispose() {
            _cache?.Dispose();
            _decorated?.Dispose();
            _sut?.Dispose();
        }

        public class Register : CachingMongoDbClientStoreTests {
            private readonly KeyId _keyId;
            private readonly string _cacheKey;
            private readonly Client _cachedClient;
            private readonly Client _newClient;
            
            public Register() {
                _keyId = new KeyId("c1");
                _cacheKey = $"CacheEntry_Client_{_keyId}";
                _cachedClient = new Client(_keyId, "cached", new CustomSignatureAlgorithm("cAlg"), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
                _newClient = new Client(_keyId, "client one", new CustomSignatureAlgorithm("cAlg"), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
            }
            
            [Fact]
            public async Task DelegatesToDecoratedInstance() {
                await _sut.Register(_newClient);

                A.CallTo(() => _decorated.Register(_newClient))
                    .MustHaveHappened();
            }

            [Fact]
            public async Task WhenClientIsNotCached_AddsEntryToCache() {
                _cache.TryGetValue(_cacheKey, out _).Should().BeFalse();

                await _sut.Register(_newClient);

                _cache.TryGetEntry(_cacheKey, out var actualEntry).Should().BeTrue();
                actualEntry.As<ICacheEntry>().AbsoluteExpirationRelativeToNow.Should().Be(_expiration);
            }
            
            [Fact]
            public async Task WhenClientIsCached_ReplacesEntryInCache() {
                var cacheEntry = _cache.CreateEntry(_cacheKey);
                cacheEntry.Value = _cachedClient;

                await _sut.Register(_newClient);

                _cache.TryGetEntry(_cacheKey, out var actualEntry).Should().BeTrue();
                actualEntry.Value.Should().Be(_newClient);
            }

            [Fact]
            public async Task WhenItemIsEvicted_DisposesClient() {
                A.CallTo(() => _backgroundTaskStarter.Start(A<Func<Task>>._, A<TimeSpan>._))
                    .Invokes(call => {
                        var func = call.GetArgument<Func<Task>>(0);
                        func.Invoke().GetAwaiter().GetResult();
                    });
                
                await _sut.Register(_newClient);
                
                // Force call eviction callbacks
                _cache.TryGetEntry(_cacheKey, out var cacheEntry);
                foreach (var callback in cacheEntry.PostEvictionCallbacks) {
                    callback.EvictionCallback.Invoke(_cacheKey, cacheEntry.Value, EvictionReason.Expired, null);
                }

                ((CustomSignatureAlgorithm) _newClient.SignatureAlgorithm).IsDisposed().Should().BeTrue();
            }
        }

        public class Get : CachingMongoDbClientStoreTests {
            private readonly KeyId _keyId;
            private readonly string _cacheKey;
            private readonly Client _cachedClient;
            private readonly Client _newClient;

            public Get() {
                _keyId = new KeyId("c1");
                _cacheKey = $"CacheEntry_Client_{_keyId}";

                _cachedClient = new Client(_keyId, "cached", new CustomSignatureAlgorithm("cAlg"), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
                _newClient = new Client(_keyId, "new", new CustomSignatureAlgorithm("cAlg"), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
                A.CallTo(() => _decorated.Get(_keyId)).Returns(_newClient);
            }

            [Theory]
            [InlineData(0)]
            [InlineData(-1)]
            [InlineData(-99)]
            public async Task WhenExpirationIsZeroOrNegative_DoesNotUseCache_AndDelegatesToDecoratedInstance(int expirationSeconds) {
                var sut = new CachingMongoDbClientStore(_decorated, _cache, TimeSpan.FromSeconds(expirationSeconds), _backgroundTaskStarter);

                await sut.Get(_keyId);

                A.CallTo(() => _decorated.Get(_keyId))
                    .MustHaveHappened();
                _cache.InternalData.Should().BeEmpty();
            }

            [Fact]
            public async Task WhenClientIsNotCached_AcquiresFromDecoratedInstance() {
                _cache.TryGetValue(_cacheKey, out _).Should().BeFalse();

                var actual = await _sut.Get(_keyId);

                actual.Should().Be(_newClient).And.Match<Client>(_ => _.Name == "new");
            }

            [Fact]
            public async Task WhenClientIsNotCached_CachesAcquiredFromDecoratedInstance() {
                _cache.TryGetValue(_cacheKey, out _).Should().BeFalse();

                await _sut.Get(_keyId);

                _cache.TryGetValue(_cacheKey, out var actualCachedValue).Should().BeTrue();
                actualCachedValue.Should().Be(_newClient).And.Match<Client>(_ => _.Name == "new");
            }

            [Fact]
            public async Task WhenClientIsNotCached_AndIsNotFound_CachesNullValue() {
                var cacheKey = "CacheEntry_Client_c2";
                _cache.TryGetValue(cacheKey, out _).Should().BeFalse();
                A.CallTo(() => _decorated.Get(new KeyId("c2"))).Returns((Client)null);
                
                var actual = await _sut.Get(new KeyId("c2"));

                actual.Should().BeNull();
                _cache.TryGetEntry(cacheKey, out var actualEntry).Should().BeTrue();
                actualEntry.As<ICacheEntry>().Value.Should().BeNull();
                actualEntry.As<ICacheEntry>().AbsoluteExpirationRelativeToNow.Should().Be(_expiration);
            }
            
            [Fact]
            public async Task WhenClientIsNotCached_CachesAcquiredFromDecoratedInstance_WithExpectedExpiration() {
                _cache.TryGetValue(_cacheKey, out _).Should().BeFalse();

                await _sut.Get(_keyId);

                _cache.TryGetEntry(_cacheKey, out var actualEntry).Should().BeTrue();
                actualEntry.As<ICacheEntry>().AbsoluteExpirationRelativeToNow.Should().Be(_expiration);
            }

            [Fact]
            public async Task WhenClientIsCached_ReturnsCachedInstance() {
                var cacheEntry = _cache.CreateEntry(_cacheKey);
                cacheEntry.Value = _cachedClient;

                var actual = await _sut.Get(_keyId);

                actual.Should().Be(_cachedClient).And.Match<Client>(_ => _.Name == "cached");
            }

            [Fact]
            public async Task WhenClientIsCached_DoesNotAcquireFromDecoratedInstance() {
                var cacheEntry = _cache.CreateEntry(_cacheKey);
                cacheEntry.Value = _cachedClient;

                await _sut.Get(_keyId);

                A.CallTo(() => _decorated.Get(_keyId))
                    .MustNotHaveHappened();
            }
        }

        public class DisposableSupport : CachingMongoDbClientStoreTests {
            [Fact]
            public void DisposesOfDecoratedInstance() {
                _sut.Dispose();

                A.CallTo(() => _decorated.Dispose())
                    .MustHaveHappened();
            }

            [Fact]
            public void DoesNotDisposeOfCache() {
                _sut.Dispose();

                _cache.IsDisposed.Should().BeFalse();
            }
        }
    }
}