using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    internal class CachingMongoDbClientStore : IMongoDbClientStore {
        private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);
        private static readonly TimeSpan MaxLockWaitTime = TimeSpan.FromSeconds(10);
        
        private readonly IMemoryCache _cache;
        private readonly IMongoDbClientStore _decorated;
        private readonly TimeSpan _expiration;

        public CachingMongoDbClientStore(IMongoDbClientStore decorated, IMemoryCache cache, TimeSpan expiration) {
            _decorated = decorated ?? throw new ArgumentNullException(nameof(decorated));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _expiration = expiration;
        }

        public Task Register(Client client) {
            return _decorated.Register(client);
        }

        public async Task<Client> Get(KeyId clientId) {
            if (_expiration <= TimeSpan.Zero) return await _decorated.Get(clientId);

            await Semaphore.WaitAsync(MaxLockWaitTime);

            var cacheKey = $"CacheEntry_Client_{clientId}";
            try {
                if (!_cache.TryGetValue<Client>(cacheKey, out var cachedClient)) {
                    var retrievedClient = await _decorated.Get(clientId);
                    _cache.Set(cacheKey, retrievedClient, _expiration);
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
    }
}