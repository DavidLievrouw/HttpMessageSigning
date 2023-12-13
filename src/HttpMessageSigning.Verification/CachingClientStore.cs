using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.Utils;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace Dalion.HttpMessageSigning.Verification {
    internal class CachingClientStore : IClientStore {
        private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(initialCount: 1, maxCount: 1);
        private static readonly TimeSpan MaxLockWaitTime = TimeSpan.FromSeconds(10);

        private readonly IMemoryCache _cache;
        private readonly IClientStore _decorated;
        private readonly Func<TimeSpan> _expiration;
        private readonly IBackgroundTaskStarter _backgroundTaskStarter;
        private readonly ConcurrentDictionary<ClientStoreCacheKey, CancellationTokenSource> _tokenSources;
        private readonly object _tokenSourcesLock = new object();

        public CachingClientStore(IClientStore decorated, IMemoryCache cache, Func<TimeSpan> expiration, IBackgroundTaskStarter backgroundTaskStarter) {
            _decorated = decorated ?? throw new ArgumentNullException(nameof(decorated));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _expiration = expiration;
            _backgroundTaskStarter = backgroundTaskStarter ?? throw new ArgumentNullException(nameof(backgroundTaskStarter));
            _tokenSources = new ConcurrentDictionary<ClientStoreCacheKey, CancellationTokenSource>();
        }

        public async Task Register(Client client) {
            if (client == null) throw new ArgumentNullException(nameof(client));
            
            Evict(client.Id);
            
            await _decorated.Register(client).ConfigureAwait(continueOnCapturedContext: false);

            var expiration = _expiration();
            if (expiration > TimeSpan.Zero) {
                var cacheKey = new ClientStoreCacheKey(client.Id);
                var options = BuildEntryOptions(cacheKey);
                _cache.Set(cacheKey, client, options);
            }
        }

        public async Task<Client> Get(KeyId clientId) {
            var expiration = _expiration();
            
            if (expiration <= TimeSpan.Zero) return await _decorated.Get(clientId).ConfigureAwait(continueOnCapturedContext: false);

            await Semaphore.WaitAsync(MaxLockWaitTime).ConfigureAwait(continueOnCapturedContext: false);

            var cacheKey = new ClientStoreCacheKey(clientId);
            try {
                var client = await _cache.GetOrCreateAsync(
                    cacheKey,
                    async entry => {
                        var options = BuildEntryOptions(cacheKey);
                        entry.SetOptions(options);

                        var retrievedClient = await _decorated.Get(clientId).ConfigureAwait(continueOnCapturedContext: false);

                        return retrievedClient;
                    }
                );

                return client;
            }
            finally {
                Semaphore.Release();
            }
        }

        public void Dispose() {
            _decorated.Dispose();
        }

        private MemoryCacheEntryOptions BuildEntryOptions(ClientStoreCacheKey cacheKey) {
            if (cacheKey == null) throw new ArgumentNullException(nameof(cacheKey));
            
            lock (_tokenSourcesLock) {
                var tokenSource = _tokenSources.GetOrAdd(cacheKey, _ => new CancellationTokenSource());
                return new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(_expiration())
                    .AddExpirationToken(new CancellationChangeToken(tokenSource.Token))
                    .RegisterPostEvictionCallback((key, value, reason, state) => {
                        var evictedClient = value as Client;
                        _backgroundTaskStarter.Start(() => {
                            evictedClient?.Dispose();
                            return Task.CompletedTask;
                        }, TimeSpan.FromSeconds(5));
                    });
            }
        }
        
        internal void Evict(KeyId id) {
            if (id == KeyId.Empty) throw new ArgumentException("Value cannot be empty.", nameof(id));
           
            var cacheKey = new ClientStoreCacheKey(id);
            
            lock (_tokenSourcesLock) {
                var tokenSource = _tokenSources.GetOrAdd(cacheKey, _ => new CancellationTokenSource());

                if (tokenSource != null && !tokenSource.IsCancellationRequested && tokenSource.Token.CanBeCanceled) {
                    tokenSource.Cancel();
                    tokenSource.Dispose();
                }

                _tokenSources.TryRemove(cacheKey, out _);
            }
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
                return _id != null
                    ? _id.GetHashCode()
                    : 0;
            }
        }
    }
}