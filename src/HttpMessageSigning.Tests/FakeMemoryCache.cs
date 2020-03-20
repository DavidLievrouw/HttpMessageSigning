using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace Dalion.HttpMessageSigning {
    public class FakeMemoryCache : IMemoryCache {
        public FakeMemoryCache() {
            InternalData = new Dictionary<object, ICacheEntry>();
        }

        public Dictionary<object, ICacheEntry> InternalData { get; }
        public bool IsDisposed { get; private set; }

        public void Dispose() {
            IsDisposed = true;
            InternalData.Clear();
        }

        public bool TryGetValue(object key, out object value) {
            if (InternalData.TryGetValue(key, out var entry)) {
                value = entry.Value;
                return true;
            }

            value = null;
            return false;
        }

        public ICacheEntry CreateEntry(object key) {
            var newEntry = new FakeCacheEntry(key);
            InternalData.Add(key, newEntry);
            return newEntry;
        }

        public void Remove(object key) {
            InternalData.Remove(key);
        }

        public bool TryGetEntry(object key, out ICacheEntry entry) {
            return InternalData.TryGetValue(key, out entry);
        }

        public class FakeCacheEntry : ICacheEntry {
            public FakeCacheEntry(object key) {
                Key = key;
            }

            public void Dispose() { }

            public object Key { get; }
            public object Value { get; set; }
            public DateTimeOffset? AbsoluteExpiration { get; set; }
            public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }
            public TimeSpan? SlidingExpiration { get; set; }
            public IList<IChangeToken> ExpirationTokens { get; }
            public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks { get; }
            public CacheItemPriority Priority { get; set; }
            public long? Size { get; set; }
        }
    }
}