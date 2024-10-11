using System;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.TestUtils;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification {
    public class CachingNonceStoreTests : IDisposable {
        private readonly FakeMemoryCache _cache;
        private readonly INonceStore _decorated;
        private readonly DateTimeOffset _now;
        private readonly CachingNonceStore _sut;

        public CachingNonceStoreTests() {
            FakeFactory.Create(out _decorated);
            _cache = new FakeMemoryCache();
            _sut = new CachingNonceStore(_decorated, _cache);

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
            _cache?.Dispose();
            _decorated?.Dispose();
            _sut?.Dispose();
        }

        public class Get : CachingNonceStoreTests {
            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public async Task GivenNullOrEmptyId_ThrowsArgumentException(string nullOrEmpty) {
                Func<Task> act = () => _sut.Get((KeyId)nullOrEmpty, "abc123");
                await act.Should().ThrowAsync<ArgumentException>();
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public async Task GivenNullOrEmptyNonceValue_ThrowsArgumentException(string nullOrEmpty) {
                Func<Task> act = () => _sut.Get(new KeyId("c1"), nullOrEmpty);
                await act.Should().ThrowAsync<ArgumentException>();
            }

            [Fact]
            public async Task WhenNonceIsInCache_AndItIsNotNull_ReturnsCachedNonce() {
                var cachedNonce = new Nonce((KeyId)"c1", "abc123", _now.AddMinutes(1));
                var cacheKey = CacheKeyFactory(cachedNonce);
                var cacheEntry = _cache.CreateEntry(cacheKey);
                cacheEntry.Value = cachedNonce;

                var actual = await _sut.Get(cachedNonce.ClientId, cachedNonce.Value);

                actual.Should().Be(cachedNonce);
            }

            [Fact]
            public async Task WhenNonceIsInCache_AndItIsNotNull_DoesNotCallDecoratedService() {
                var cachedNonce = new Nonce((KeyId)"c1", "abc123", _now.AddMinutes(1));
                var cacheKey = CacheKeyFactory(cachedNonce);
                var cacheEntry = _cache.CreateEntry(cacheKey);
                cacheEntry.Value = cachedNonce;

                await _sut.Get(cachedNonce.ClientId, cachedNonce.Value);

                A.CallTo(() => _decorated.Get(A<KeyId>._, A<string>._))
                    .MustNotHaveHappened();
            }

            [Fact]
            public async Task WhenNonceIsInCache_ButItIsNull_AndNonceIsResolved_AddsToCacheWithExpectedExpiration() {
                var cacheKey = CacheKeyFactory((KeyId)"c1", "abc123");
                var cacheEntry = _cache.CreateEntry(cacheKey);
                cacheEntry.Value = null;

                var resolvedNonce = new Nonce((KeyId)"c1", "abc123", _now.AddMinutes(2));
                A.CallTo(() => _decorated.Get((KeyId)"c1", "abc123"))
                    .Returns(resolvedNonce);

                await _sut.Get((KeyId)"c1", "abc123");

                _cache.TryGetEntry(cacheKey, out var actualEntry).Should().BeTrue();
                actualEntry.As<ICacheEntry>().AbsoluteExpiration.Should().Be(resolvedNonce.Expiration);
            }

            [Fact]
            public async Task WhenNonceIsInCache_ButItIsNull_AndNonceIsResolved_ReturnsResolvedNonce() {
                var cacheKey = CacheKeyFactory((KeyId)"c1", "abc123");
                var cacheEntry = _cache.CreateEntry(cacheKey);
                cacheEntry.Value = null;

                var resolvedNonce = new Nonce((KeyId)"c1", "abc123", _now.AddMinutes(2));
                A.CallTo(() => _decorated.Get((KeyId)"c1", "abc123"))
                    .Returns(resolvedNonce);

                var actual = await _sut.Get((KeyId)"c1", "abc123");

                actual.Should().Be(resolvedNonce);
            }

            [Fact]
            public async Task WhenNonceIsInCache_ButItIsNull_AndNonceCannotBeResolved_ReturnsNull() {
                var cacheKey = CacheKeyFactory((KeyId)"c1", "abc123");
                var cacheEntry = _cache.CreateEntry(cacheKey);
                cacheEntry.Value = null;

                A.CallTo(() => _decorated.Get((KeyId)"c1", "abc123"))
                    .Returns((Nonce) null);

                var actual = await _sut.Get((KeyId)"c1", "abc123");

                actual.Should().BeNull();
            }

            [Fact]
            public async Task WhenNonceIsNotCached_AndNonceIsResolved_ReturnsResolvedNonce() {
                var cacheKey = CacheKeyFactory((KeyId)"c1", "abc123");
                _cache.TryGetValue(cacheKey, out _).Should().BeFalse();

                var resolvedNonce = new Nonce((KeyId)"c1", "abc123", _now.AddMinutes(2));
                A.CallTo(() => _decorated.Get((KeyId)"c1", "abc123"))
                    .Returns(resolvedNonce);

                var actual = await _sut.Get((KeyId)"c1", "abc123");

                actual.Should().Be(resolvedNonce);
            }

            [Fact]
            public async Task WhenNonceIsNotCached_AndItIsResolved_AddsToCacheWithExpectedExpiration() {
                var cacheKey = CacheKeyFactory((KeyId)"c1", "abc123");
                _cache.TryGetValue(cacheKey, out _).Should().BeFalse();

                var resolvedNonce = new Nonce((KeyId)"c1", "abc123", _now.AddMinutes(2));
                A.CallTo(() => _decorated.Get((KeyId)"c1", "abc123"))
                    .Returns(resolvedNonce);

                await _sut.Get((KeyId)"c1", "abc123");

                _cache.TryGetEntry(cacheKey, out var actualEntry).Should().BeTrue();
                actualEntry.As<ICacheEntry>().AbsoluteExpiration.Should().Be(resolvedNonce.Expiration);
            }

            [Fact]
            public async Task WhenNonceIsNotCached_AndNonceCannotBeResolved_ReturnsNull() {
                var cacheKey = CacheKeyFactory((KeyId)"c1", "abc123");
                _cache.TryGetValue(cacheKey, out _).Should().BeFalse();

                A.CallTo(() => _decorated.Get((KeyId)"c1", "abc123"))
                    .Returns((Nonce) null);

                var actual = await _sut.Get((KeyId)"c1", "abc123");

                actual.Should().BeNull();
            }

            [Fact]
            public async Task WhenNonceIsNotCached_AndNonceCannotNotResolved_DoesNotAddToCache() {
                var cacheKey = CacheKeyFactory((KeyId)"c1", "abc123");
                _cache.TryGetValue(cacheKey, out _).Should().BeFalse();

                A.CallTo(() => _decorated.Get((KeyId)"c1", "abc123"))
                    .Returns((Nonce) null);

                await _sut.Get((KeyId)"c1", "abc123");

                _cache.TryGetEntry(cacheKey, out _).Should().BeFalse();
            }
        }

        public class Register : CachingNonceStoreTests {
            [Fact]
            public async Task DelegatesToDecoratedInstance() {
                var nonce = new Nonce(new KeyId("c1"), "abc123", _now.AddSeconds(30));

                await _sut.Register(nonce);

                A.CallTo(() => _decorated.Register(nonce))
                    .MustHaveHappened();
            }

            [Fact]
            public async Task AddsToCacheWithExpectedExpiration() {
                var nonce = new Nonce(new KeyId("c1"), "abc123", _now.AddSeconds(30));

                var cacheKey = CacheKeyFactory(nonce);
                _cache.TryGetValue(cacheKey, out _).Should().BeFalse();

                await _sut.Register(nonce);

                _cache.TryGetEntry(cacheKey, out var actualEntry).Should().BeTrue();
                actualEntry.As<ICacheEntry>().Value.Should().Be(nonce);
                actualEntry.As<ICacheEntry>().AbsoluteExpiration.Should().Be(nonce.Expiration);
            }
        }

        public class DisposableSupport : CachingNonceStoreTests {
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

        private static object CacheKeyFactory(KeyId clientId, string nonceValue) {
            return new CachingNonceStore.CachingNonceStoreCacheKey(clientId, nonceValue);
        }

        private static object CacheKeyFactory(Nonce nonce) {
            return new CachingNonceStore.CachingNonceStoreCacheKey(nonce.ClientId, nonce.Value);
        }
    }
}