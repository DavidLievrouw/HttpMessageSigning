using System;
using System.Threading;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.Utils;
using Microsoft.Extensions.Caching.Memory;

namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    internal class CachingMongoDbClientStore : IMongoDbClientStore {
        private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);
        private static readonly TimeSpan MaxLockWaitTime = TimeSpan.FromSeconds(10);

        private readonly IMemoryCache _cache;
        private readonly IMongoDbClientStore _decorated;
        private readonly TimeSpan _expiration;
        private readonly IBackgroundTaskStarter _backgroundTaskStarter;

        public CachingMongoDbClientStore(IMongoDbClientStore decorated, IMemoryCache cache, TimeSpan expiration, IBackgroundTaskStarter backgroundTaskStarter) {
            _decorated = decorated ?? throw new ArgumentNullException(nameof(decorated));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _expiration = expiration;
            _backgroundTaskStarter = backgroundTaskStarter ?? throw new ArgumentNullException(nameof(backgroundTaskStarter));
        }

        public async Task Register(Client client) {
            await _decorated.Register(client).ConfigureAwait(continueOnCapturedContext: false);

            if (_expiration > TimeSpan.Zero) {
                var cacheKey = $"CacheEntry_Client_{client.Id}";
                var options = BuildEntryOptions();
                _cache.Set(cacheKey, client, options);
            }
        }

        public async Task<Client> Get(KeyId clientId) {
            if (_expiration <= TimeSpan.Zero) return await _decorated.Get(clientId).ConfigureAwait(continueOnCapturedContext: false);

            await Semaphore.WaitAsync(MaxLockWaitTime).ConfigureAwait(continueOnCapturedContext: false);

            var cacheKey = $"CacheEntry_Client_{clientId}";
            try {
                if (!_cache.TryGetValue<Client>(cacheKey, out var cachedClient)) {
                    var retrievedClient = await _decorated.Get(clientId).ConfigureAwait(continueOnCapturedContext: false);
                    var options = BuildEntryOptions();
                    _cache.Set(cacheKey, retrievedClient, options);
                    return retrievedClient;
                }

                return cachedClient;
            }
            finally {
                Semaphore.Release();
            }
        }

        public void Dispose() {
            _decorated.Dispose();
        }

        private MemoryCacheEntryOptions BuildEntryOptions() {
            return new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(_expiration)
                .RegisterPostEvictionCallback((key, value, reason, state) => {
                    var evictedClient = value as Client;
                    _backgroundTaskStarter.Start(() => {
                        evictedClient?.Dispose();
                        return Task.CompletedTask;
                    }, TimeSpan.FromSeconds(5));
                });
        }
    }
}