using System;
using System.Threading;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.Utils;
using Microsoft.Extensions.Caching.Memory;

namespace Dalion.HttpMessageSigning.Verification {
    internal class CachingClientStore : ICachingClientStore {
        private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);
        private static readonly TimeSpan MaxLockWaitTime = TimeSpan.FromSeconds(10);

        private readonly IBackgroundTaskStarter _backgroundTaskStarter;
        private readonly IMemoryCache _cache;
        private readonly IClientStore _decorated;
        private readonly Func<TimeSpan> _expiration;

        public CachingClientStore(IClientStore decorated, IMemoryCache cache, Func<TimeSpan> expiration, IBackgroundTaskStarter backgroundTaskStarter) {
            _decorated = decorated ?? throw new ArgumentNullException(nameof(decorated));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _expiration = expiration;
            _backgroundTaskStarter = backgroundTaskStarter ?? throw new ArgumentNullException(nameof(backgroundTaskStarter));
        }

        public async Task Register(Client client) {
            if (client == null) throw new ArgumentNullException(nameof(client));

            await Semaphore.WaitAsync(MaxLockWaitTime).ConfigureAwait(false);

            try {
                EvictNoLock(client.Id);

                await _decorated.Register(client).ConfigureAwait(false);
            }
            finally {
                Semaphore.Release();
            }
        }

        public async Task<Client> Get(KeyId clientId) {
            var expiration = _expiration();

            if (expiration <= TimeSpan.Zero) return await _decorated.Get(clientId).ConfigureAwait(false);

            await Semaphore.WaitAsync(MaxLockWaitTime).ConfigureAwait(false);

            var cacheKey = new ClientStoreCacheKey(clientId);
            try {
                var client = await _cache
                    .GetOrCreateAsync(
                        cacheKey,
                        async entry => {
                            var options = BuildEntryOptions();
                            entry.SetOptions(options);

                            var retrievedClient = await _decorated.Get(clientId).ConfigureAwait(false);

                            return retrievedClient;
                        }
                    )
                    .ConfigureAwait(false);

                return client;
            }
            finally {
                Semaphore.Release();
            }
        }

        public void Dispose() {
            _decorated.Dispose();
        }

        public void Evict(KeyId id) {
            if (id == KeyId.Empty) throw new ArgumentException("Value cannot be empty.", nameof(id));

            Semaphore.Wait(MaxLockWaitTime);
            try {
                EvictNoLock(id);
            }
            finally {
                Semaphore.Release();
            }
        }

        private void EvictNoLock(KeyId id) {
            if (id == KeyId.Empty) throw new ArgumentException("Value cannot be empty.", nameof(id));

            var cacheKey = new ClientStoreCacheKey(id);
            _cache.Remove(cacheKey);
        }

        private MemoryCacheEntryOptions BuildEntryOptions() {
            return new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(_expiration())
                .RegisterPostEvictionCallback(
                    (key, value, reason, state) => {
                        var evictedClient = value as Client;
                        _backgroundTaskStarter.Start(
                            () => {
                                evictedClient?.Dispose();
                                return Task.CompletedTask;
                            },
                            TimeSpan.FromSeconds(5)
                        );
                    }
                );
        }

        // Internal for testing purposes
        internal class ClientStoreCacheKey : IEquatable<ClientStoreCacheKey> {
            private readonly KeyId _id;

            public ClientStoreCacheKey(KeyId id) {
                if (id == KeyId.Empty) throw new ArgumentException("Value cannot be empty.", nameof(id));
                _id = id;
            }

            public bool Equals(ClientStoreCacheKey other) {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return _id == other._id;
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj.GetType() == typeof(ClientStoreCacheKey) && Equals((ClientStoreCacheKey)obj);
            }

            public override int GetHashCode() {
                return _id.GetHashCode();
            }
        }
    }
}