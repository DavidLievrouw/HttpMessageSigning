using System;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.TestUtils;
using Dalion.HttpMessageSigning.Utils;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification {
    public class CachingClientStoreTests : IDisposable {
        private readonly IMemoryCache _cache;
        private readonly IClientStore _decorated;
        private TimeSpan _expiration;
        private readonly IBackgroundTaskStarter _backgroundTaskStarter;
        private readonly CachingClientStore _sut;
        private readonly ServiceProvider _provider;

        public CachingClientStoreTests() {
            FakeFactory.Create(out _decorated, out _backgroundTaskStarter);

            _provider = new ServiceCollection()
                .AddMemoryCache()
                .BuildServiceProvider();
            _cache = _provider.GetRequiredService<IMemoryCache>();
            _expiration = TimeSpan.FromSeconds(30);

            _sut = new CachingClientStore(_decorated, _cache, () => _expiration, _backgroundTaskStarter);
        }

        public void Dispose() {
            _cache?.Dispose();
            _decorated?.Dispose();
            _provider?.Dispose();
            _sut?.Dispose();
        }

        public class Register : CachingClientStoreTests {
            private readonly CachingClientStore.ClientStoreCacheKey _cacheKey;
            private readonly Client _cachedClient;
            private readonly Client _newClient;

            public Register() {
                var keyId = new KeyId("c1");
                _cacheKey = new CachingClientStore.ClientStoreCacheKey(keyId);
                _cachedClient = new Client(
                    keyId,
                    "cached",
                    new CustomSignatureAlgorithm("cAlg"),
                    TimeSpan.FromMinutes(1),
                    TimeSpan.FromMinutes(1),
                    RequestTargetEscaping.RFC3986);
                _newClient = new Client(
                    keyId,
                    "client one",
                    new CustomSignatureAlgorithm("cAlg"),
                    TimeSpan.FromMinutes(1),
                    TimeSpan.FromMinutes(1),
                    RequestTargetEscaping.RFC3986);
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

                _cache.TryGetValue(_cacheKey, out _).Should().BeTrue();
            }

            [Fact]
            public async Task WhenClientIsCached_ReplacesEntryInCache() {
                _cache.Set(_cacheKey, _cachedClient);

                await _sut.Register(_newClient);

                _cache.TryGetValue(_cacheKey, out var actual).Should().BeTrue();
                actual.Should().Be(_newClient);
            }

            [Fact]
            public async Task WhenItemIsEvicted_DisposesClient() {
                A.CallTo(() => _backgroundTaskStarter.Start(A<Func<Task>>._, A<TimeSpan>._))
                    .Invokes(call => {
                        var func = call.GetArgument<Func<Task>>(0);
#pragma warning disable xUnit1031
                        func.Invoke().GetAwaiter().GetResult();
#pragma warning restore xUnit1031
                    });

                await _sut.Register(_newClient);

                // Force eviction
                _sut.Evict(_cachedClient.Id);

                await Task.Delay(TimeSpan.FromSeconds(1));

                ((CustomSignatureAlgorithm)_newClient.SignatureAlgorithm).IsDisposed().Should().BeTrue();
            }

            [Theory]
            [InlineData(0)]
            [InlineData(-1)]
            [InlineData(-99)]
            public async Task WhenExpirationIsZeroOrNegative_DoesNotUseCache_AndDelegatesToDecoratedInstance(int expirationSeconds) {
                _expiration = TimeSpan.FromSeconds(expirationSeconds);

                await _sut.Register(_newClient);

                A.CallTo(() => _decorated.Register(_newClient))
                    .MustHaveHappened();
                _cache.TryGetValue(_cacheKey, out _).Should().BeFalse();
            }
        }

        public class Get : CachingClientStoreTests {
            private readonly KeyId _keyId;
            private readonly CachingClientStore.ClientStoreCacheKey _cacheKey;
            private readonly Client _cachedClient;
            private readonly Client _newClient;

            public Get() {
                _keyId = new KeyId("c1");
                _cacheKey = new CachingClientStore.ClientStoreCacheKey(_keyId);

                _cachedClient = new Client(
                    _keyId,
                    "cached",
                    new CustomSignatureAlgorithm("cAlg"),
                    TimeSpan.FromMinutes(1),
                    TimeSpan.FromMinutes(1),
                    RequestTargetEscaping.RFC3986);
                _newClient = new Client(
                    _keyId,
                    "new",
                    new CustomSignatureAlgorithm("cAlg"),
                    TimeSpan.FromMinutes(1),
                    TimeSpan.FromMinutes(1),
                    RequestTargetEscaping.RFC3986);
                A.CallTo(() => _decorated.Get(_keyId)).Returns(_newClient);
            }

            [Theory]
            [InlineData(0)]
            [InlineData(-1)]
            [InlineData(-99)]
            public async Task WhenExpirationIsZeroOrNegative_DoesNotUseCache_AndDelegatesToDecoratedInstance(int expirationSeconds) {
                _expiration = TimeSpan.FromSeconds(expirationSeconds);

                await _sut.Get(_keyId);

                A.CallTo(() => _decorated.Get(_keyId))
                    .MustHaveHappened();
                _cache.TryGetValue(_cacheKey, out _).Should().BeFalse();
            }

            [Fact]
            public async Task WhenClientIsNotCached_AcquiresFromDecoratedInstance() {
                _cache.TryGetValue(_cacheKey, out _).Should().BeFalse();

                var actual = await _sut.Get(_keyId);

                actual.Should().Be(_newClient).And.Match<Client>(c => c.Name == "new");
            }

            [Fact]
            public async Task WhenClientIsNotCached_CachesAcquiredFromDecoratedInstance() {
                _cache.TryGetValue(_cacheKey, out _).Should().BeFalse();

                await _sut.Get(_keyId);

                _cache.TryGetValue(_cacheKey, out var actualCachedValue).Should().BeTrue();
                actualCachedValue.Should().Be(_newClient).And.Match<Client>(c => c.Name == "new");
            }

            [Fact]
            public async Task WhenClientIsNotCached_AndIsNotFound_CachesNullValue() {
                var otherKeyId = new KeyId("c2");

                var cacheKey = new CachingClientStore.ClientStoreCacheKey(otherKeyId);
                _cache.TryGetValue(cacheKey, out _).Should().BeFalse();

                A.CallTo(() => _decorated.Get(otherKeyId)).Returns((Client)null);

                var actual = await _sut.Get(otherKeyId);

                actual.Should().BeNull();

                _cache.TryGetValue(cacheKey, out var actualEntry).Should().BeTrue();
                actualEntry.Should().BeNull();
            }

            [Fact]
            public async Task WhenClientIsCached_ReturnsCachedInstance() {
                _cache.Set(_cacheKey, _cachedClient);

                var actual = await _sut.Get(_keyId);

                actual.Should().Be(_cachedClient).And.Match<Client>(c => c.Name == "cached");
            }

            [Fact]
            public async Task WhenClientIsCached_DoesNotAcquireFromDecoratedInstance() {
                _cache.Set(_cacheKey, _cachedClient);

                await _sut.Get(_keyId);

                A.CallTo(() => _decorated.Get(_keyId))
                    .MustNotHaveHappened();
            }
        }

        public class Evict : CachingClientStoreTests {
            private KeyId _keyId;
            private readonly Action _act;

            public Evict() {
                _keyId = (KeyId)"c1";
                _act = () => _sut.Evict(_keyId);
            }

            [Fact]
            public void WhenEmptyKeyId_ThrowsArgumentException() {
                _keyId = KeyId.Empty;

                _act.Should().Throw<ArgumentException>();
            }

            [Fact]
            public void WhenNotCached_DoesNotThrow() {
                _act.Should().NotThrow();
            }

            [Fact]
            public async Task WhenCached_EvictsFromCache() {
                var cacheKey = new CachingClientStore.ClientStoreCacheKey(_keyId);
                var client = new Client(
                    _keyId,
                    "cached",
                    new CustomSignatureAlgorithm("cAlg"),
                    TimeSpan.FromMinutes(1),
                    TimeSpan.FromMinutes(1),
                    RequestTargetEscaping.RFC3986);
                
                await _sut.Register(client);

                _cache.TryGetValue(cacheKey, out _).Should().BeTrue();

                _act();

                _cache.TryGetValue(cacheKey, out _).Should().BeFalse();
            }

            [Fact]
            public async Task WhenItemIsEvicted_DisposesClient() {
                A.CallTo(() => _backgroundTaskStarter.Start(A<Func<Task>>._, A<TimeSpan>._))
                    .Invokes(call => {
                        var func = call.GetArgument<Func<Task>>(0);
#pragma warning disable xUnit1031
                        func.Invoke().GetAwaiter().GetResult();
#pragma warning restore xUnit1031
                    });

                var cacheKey = new CachingClientStore.ClientStoreCacheKey(_keyId);
                var client = new Client(
                    _keyId,
                    "cached",
                    new CustomSignatureAlgorithm("cAlg"),
                    TimeSpan.FromMinutes(1),
                    TimeSpan.FromMinutes(1),
                    RequestTargetEscaping.RFC3986);
                
                await _sut.Register(client);

                _cache.TryGetValue(cacheKey, out _).Should().BeTrue();

                _act();

                await Task.Delay(TimeSpan.FromSeconds(1));

                ((CustomSignatureAlgorithm)client.SignatureAlgorithm).IsDisposed().Should().BeTrue();
            }
        }

        public class DisposableSupport : CachingClientStoreTests {
            [Fact]
            public void DisposesOfDecoratedInstance() {
                _sut.Dispose();

                A.CallTo(() => _decorated.Dispose())
                    .MustHaveHappened();
            }
        }
    }
}