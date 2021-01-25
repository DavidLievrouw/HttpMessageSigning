using System;
using Dalion.HttpMessageSigning.Utils;
using Microsoft.Extensions.Caching.Memory;

namespace Dalion.HttpMessageSigning.Verification {
    internal class CachingClientStoreDecorator : ICachingClientStoreDecorator {
        private readonly IMemoryCache _cache;
        private readonly IBackgroundTaskStarter _backgroundTaskStarter;

        public CachingClientStoreDecorator(IMemoryCache cache, IBackgroundTaskStarter backgroundTaskStarter) {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _backgroundTaskStarter = backgroundTaskStarter ?? throw new ArgumentNullException(nameof(backgroundTaskStarter));
        }

        public IClientStore DecorateWithCaching(IClientStore decorated, TimeSpan cacheExpiration) {
            return new CachingClientStore(decorated, _cache, cacheExpiration, _backgroundTaskStarter);
        }
    }
}