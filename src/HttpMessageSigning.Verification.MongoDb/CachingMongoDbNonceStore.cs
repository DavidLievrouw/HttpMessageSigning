using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    internal class CachingMongoDbNonceStore : IMongoDbNonceStore {
        private readonly INonceStore _decorated;
        private readonly IMemoryCache _cache;

        public CachingMongoDbNonceStore(INonceStore decorated, IMemoryCache cache) {
            _decorated = decorated ?? throw new ArgumentNullException(nameof(decorated));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public Task Register(Nonce nonce) {
            if (nonce == null) throw new ArgumentNullException(nameof(nonce));
            
            var cacheKey = CacheKeyFactory(nonce.ClientId, nonce.Value);
            _cache.Set(cacheKey, nonce, nonce.Expiration);
            
            return _decorated.Register(nonce);
        }

        public async Task<Nonce> Get(KeyId clientId, string nonceValue) {
            if (clientId == KeyId.Empty) throw new ArgumentException("Value cannot be null or empty.", nameof(clientId));
            if (string.IsNullOrEmpty(nonceValue)) throw new ArgumentException("Value cannot be null or empty.", nameof(nonceValue));
            
            var cacheKey = CacheKeyFactory(clientId, nonceValue);
            var isInCache = _cache.TryGetValue<Nonce>(cacheKey, out var cachedNonce);
            
            if (isInCache && cachedNonce != null) {
                return cachedNonce;
            }

            var nonce = await _decorated.Get(clientId, nonceValue).ConfigureAwait(continueOnCapturedContext: false);
            
            if (nonce != null) {
                _cache.Set(cacheKey, nonce, nonce.Expiration);
            }

            return nonce;
        }

        public void Dispose() {
            _decorated.Dispose();
        }

        private static string CacheKeyFactory(KeyId clientId, string nonceValue) {
            return $"CacheEntry_Nonce_{clientId}_{nonceValue ?? "null"}";
        }
    }
}