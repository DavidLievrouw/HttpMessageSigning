using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Dalion.HttpMessageSigning.Verification {
    /// <summary>
    ///     Represents a <see cref="INonceStore" /> implementation that stores clients in memory.
    /// </summary>
    public class InMemoryNonceStore : INonceStore {
        private readonly IMemoryCache _cache;

        /// <summary>
        ///     Create a new empty instance of the <see cref="InMemoryNonceStore" /> class.
        /// </summary>
        public InMemoryNonceStore(IMemoryCache cache) {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() {
            // Noop
        }

        /// <summary>
        ///     Registers usage of the specified <see cref="Nonce" /> value.
        /// </summary>
        /// <param name="nonce">The <see cref="Nonce" /> that is received from a client.</param>
        public Task Register(Nonce nonce) {
            if (nonce == null) throw new ArgumentNullException(nameof(nonce));

            var cacheKey = CacheKeyCreator(nonce.ClientId, nonce.Value);
            _cache.Set(cacheKey, nonce, nonce.Expiration);

            return Task.CompletedTask;
        }

        /// <summary>
        ///     Gets the <see cref="Nonce" /> value with a matching string, that was previously sent by the <see cref="Client" />,
        ///     identified by the specified <see cref="KeyId" />.
        /// </summary>
        /// <param name="clientId">The identifier of the client to get the value for.</param>
        /// <param name="nonceValue">The nonce string value that was sent.</param>
        /// <returns></returns>
        public Task<Nonce> Get(KeyId clientId, string nonceValue) {
            if (clientId == KeyId.Empty) throw new ArgumentException("Value cannot be null or empty.", nameof(clientId));
            if (string.IsNullOrEmpty(nonceValue)) throw new ArgumentException("Value cannot be null or empty.", nameof(nonceValue));

            var cacheKey = CacheKeyCreator(clientId, nonceValue);
            var match = _cache.Get<Nonce>(cacheKey);

            return Task.FromResult(match);
        }

        private static InMemoryNonceStoreCacheKey CacheKeyCreator(KeyId clientId, string nonce) {
            return new InMemoryNonceStoreCacheKey(clientId, nonce);
        }

        private class InMemoryNonceStoreCacheKey : IEquatable<InMemoryNonceStoreCacheKey> {
            private readonly KeyId _clientId;
            private readonly string _nonceValue;

            public InMemoryNonceStoreCacheKey(KeyId clientId, string nonceValue) {
                _clientId = clientId;
                _nonceValue = nonceValue;
            }

            public bool Equals(InMemoryNonceStoreCacheKey other) {
                if (other is null) return false;
                if (ReferenceEquals(this, other)) return true;
                return _clientId.Equals(other._clientId) && _nonceValue == other._nonceValue;
            }

            public override bool Equals(object obj) {
                return ReferenceEquals(this, obj) || (obj is InMemoryNonceStoreCacheKey other && Equals(other));
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