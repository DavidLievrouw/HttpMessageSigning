using System;

namespace Dalion.HttpMessageSigning.Verification {
    /// <summary>
    ///     Represents an object that decorates a <see cref="IClientStore" /> with a caching layer.
    /// </summary>
    public interface ICachingClientStoreDecorator {
        /// <summary>
        ///     Decorate the specified <see cref="IClientStore" /> with a caching layer.
        /// </summary>
        /// <param name="decorated">The <see cref="IClientStore" /> to decorate.</param>
        /// <param name="cacheExpiration">The expiration time of cache entries.</param>
        /// <returns>The decorated <see cref="IClientStore" />.</returns>
        IClientStore DecorateWithCaching(IClientStore decorated, TimeSpan cacheExpiration);
    }
}