using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Dalion.HttpMessageSigning.Verification {
    internal class CachingNonceStore : INonceStore {
        private readonly INonceStore _decorated;
        private readonly IMemoryCache _cache;

        public CachingNonceStore(INonceStore decorated, IMemoryCache cache) {
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

        internal static CachingNonceStoreCacheKey CacheKeyFactory(KeyId clientId, string nonceValue) {
            return new CachingNonceStoreCacheKey(clientId, nonceValue);
        }
        
        internal class CachingNonceStoreCacheKey : IEquatable<CachingNonceStoreCacheKey> {
            private readonly KeyId _clientId;
            private readonly string _nonceValue;

            public CachingNonceStoreCacheKey(KeyId clientId, string nonceValue) {
                _clientId = clientId;
                _nonceValue = nonceValue;
            }

            public bool Equals(CachingNonceStoreCacheKey other) {
                if (other is null) return false;
                if (ReferenceEquals(this, other)) return true;
                return _clientId.Equals(other._clientId) && _nonceValue == other._nonceValue;
            }

            public override bool Equals(object obj) {
                return ReferenceEquals(this, obj) || (obj is CachingNonceStoreCacheKey other && Equals(other));
            }

            public override int GetHashCode() {
                unchecked {
                    return (_clientId.GetHashCode() * 397) ^ _nonceValue.GetHashCode();
                }
            }

            public override string ToString() {
                return $"{_clientId}_{_nonceValue}";
            }
        }
    }
}