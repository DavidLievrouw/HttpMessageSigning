using System;
using Microsoft.Extensions.Caching.Memory;

namespace Dalion.HttpMessageSigning.Verification {
    internal class CachingNonceStoreDecorator : ICachingNonceStoreDecorator {
        private readonly IMemoryCache _cache;

        public CachingNonceStoreDecorator(IMemoryCache cache) {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public INonceStore DecorateWithCaching(INonceStore decorated) {
            return new CachingNonceStore(decorated, _cache);
        }
    }
}