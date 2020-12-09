using System;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.TestUtils;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    public class CachingSqlServerNonceStoreTests : IDisposable {
        private readonly FakeMemoryCache _cache;
        private readonly ISqlServerNonceStore _decorated;
        private readonly DateTimeOffset _now;
        private readonly CachingSqlServerNonceStore _sut;

        public CachingSqlServerNonceStoreTests() {
            FakeFactory.Create(out _decorated);
            _cache = new FakeMemoryCache();
            _sut = new CachingSqlServerNonceStore(_decorated, _cache);

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

        public class Get : CachingSqlServerNonceStoreTests {
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
            public async Task WhenNonceIsInCache_AndItIsNotNull_ReturnsCachedNonce() {
                var cachedNonce = new Nonce("c1", "abc123", _now.AddMinutes(1));
                var cacheKey = CacheKeyFactory(cachedNonce);
                var cacheEntry = _cache.CreateEntry(cacheKey);
                cacheEntry.Value = cachedNonce;

                var actual = await _sut.Get(cachedNonce.ClientId, cachedNonce.Value);

                actual.Should().Be(cachedNonce);
            }

            [Fact]
            public async Task WhenNonceIsInCache_AndItIsNotNull_DoesNotCallDecoratedService() {
                var cachedNonce = new Nonce("c1", "abc123", _now.AddMinutes(1));
                var cacheKey = CacheKeyFactory(cachedNonce);
                var cacheEntry = _cache.CreateEntry(cacheKey);
                cacheEntry.Value = cachedNonce;

                await _sut.Get(cachedNonce.ClientId, cachedNonce.Value);

                A.CallTo(() => _decorated.Get(A<KeyId>._, A<string>._))
                    .MustNotHaveHappened();
            }

            [Fact]
            public async Task WhenNonceIsInCache_ButItIsNull_AndNonceIsResolved_AddsToCacheWithExpectedExpiration() {
                var cacheKey = CacheKeyFactory("c1", "abc123");
                var cacheEntry = _cache.CreateEntry(cacheKey);
                cacheEntry.Value = null;

                var resolvedNonce = new Nonce("c1", "abc123", _now.AddMinutes(2));
                A.CallTo(() => _decorated.Get("c1", "abc123"))
                    .Returns(resolvedNonce);

                await _sut.Get("c1", "abc123");

                _cache.TryGetEntry(cacheKey, out var actualEntry).Should().BeTrue();
                actualEntry.As<ICacheEntry>().AbsoluteExpiration.Should().Be(resolvedNonce.Expiration);
            }

            [Fact]
            public async Task WhenNonceIsInCache_ButItIsNull_AndNonceIsResolved_ReturnsResolvedNonce() {
                var cacheKey = CacheKeyFactory("c1", "abc123");
                var cacheEntry = _cache.CreateEntry(cacheKey);
                cacheEntry.Value = null;

                var resolvedNonce = new Nonce("c1", "abc123", _now.AddMinutes(2));
                A.CallTo(() => _decorated.Get("c1", "abc123"))
                    .Returns(resolvedNonce);

                var actual = await _sut.Get("c1", "abc123");

                actual.Should().Be(resolvedNonce);
            }

            [Fact]
            public async Task WhenNonceIsInCache_ButItIsNull_AndNonceCannotBeResolved_ReturnsNull() {
                var cacheKey = CacheKeyFactory("c1", "abc123");
                var cacheEntry = _cache.CreateEntry(cacheKey);
                cacheEntry.Value = null;

                A.CallTo(() => _decorated.Get("c1", "abc123"))
                    .Returns((Nonce) null);

                var actual = await _sut.Get("c1", "abc123");

                actual.Should().BeNull();
            }

            [Fact]
            public async Task WhenNonceIsNotCached_AndNonceIsResolved_ReturnsResolvedNonce() {
                var cacheKey = CacheKeyFactory("c1", "abc123");
                _cache.TryGetValue(cacheKey, out _).Should().BeFalse();

                var resolvedNonce = new Nonce("c1", "abc123", _now.AddMinutes(2));
                A.CallTo(() => _decorated.Get("c1", "abc123"))
                    .Returns(resolvedNonce);

                var actual = await _sut.Get("c1", "abc123");

                actual.Should().Be(resolvedNonce);
            }

            [Fact]
            public async Task WhenNonceIsNotCached_AndItIsResolved_AddsToCacheWithExpectedExpiration() {
                var cacheKey = CacheKeyFactory("c1", "abc123");
                _cache.TryGetValue(cacheKey, out _).Should().BeFalse();

                var resolvedNonce = new Nonce("c1", "abc123", _now.AddMinutes(2));
                A.CallTo(() => _decorated.Get("c1", "abc123"))
                    .Returns(resolvedNonce);

                await _sut.Get("c1", "abc123");

                _cache.TryGetEntry(cacheKey, out var actualEntry).Should().BeTrue();
                actualEntry.As<ICacheEntry>().AbsoluteExpiration.Should().Be(resolvedNonce.Expiration);
            }

            [Fact]
            public async Task WhenNonceIsNotCached_AndNonceCannotBeResolved_ReturnsNull() {
                var cacheKey = CacheKeyFactory("c1", "abc123");
                _cache.TryGetValue(cacheKey, out _).Should().BeFalse();

                A.CallTo(() => _decorated.Get("c1", "abc123"))
                    .Returns((Nonce) null);

                var actual = await _sut.Get("c1", "abc123");

                actual.Should().BeNull();
            }

            [Fact]
            public async Task WhenNonceIsNotCached_AndNonceCannotNotResolved_DoesNotAddToCache() {
                var cacheKey = CacheKeyFactory("c1", "abc123");
                _cache.TryGetValue(cacheKey, out _).Should().BeFalse();

                A.CallTo(() => _decorated.Get("c1", "abc123"))
                    .Returns((Nonce) null);

                await _sut.Get("c1", "abc123");

                _cache.TryGetEntry(cacheKey, out _).Should().BeFalse();
            }

            private static string CacheKeyFactory(KeyId clientId, string nonceValue) {
                return $"CacheEntry_Nonce_{clientId}_{nonceValue}";
            }

            private static string CacheKeyFactory(Nonce nonce) {
                return $"CacheEntry_Nonce_{nonce.ClientId}_{nonce.Value}";
            }
        }

        public class Register : CachingSqlServerNonceStoreTests {
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

                var cacheKey = "CacheEntry_Nonce_c1_abc123";
                _cache.TryGetValue(cacheKey, out _).Should().BeFalse();

                await _sut.Register(nonce);

                _cache.TryGetEntry(cacheKey, out var actualEntry).Should().BeTrue();
                actualEntry.As<ICacheEntry>().Value.Should().Be(nonce);
                actualEntry.As<ICacheEntry>().AbsoluteExpiration.Should().Be(nonce.Expiration);
            }
        }

        public class DisposableSupport : CachingSqlServerNonceStoreTests {
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