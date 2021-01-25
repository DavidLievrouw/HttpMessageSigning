namespace Dalion.HttpMessageSigning.Verification {
    /// <summary>
    ///     Represents an object that decorates a <see cref="INonceStore" /> with a caching layer.
    /// </summary>
    public interface ICachingNonceStoreDecorator {
        /// <summary>
        ///     Decorate the specified <see cref="INonceStore" /> with a caching layer.
        /// </summary>
        /// <param name="decorated">The <see cref="INonceStore" /> to decorate.</param>
        /// <returns>The decorated <see cref="INonceStore" />.</returns>
        INonceStore DecorateWithCaching(INonceStore decorated);
    }
}