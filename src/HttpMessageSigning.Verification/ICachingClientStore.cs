namespace Dalion.HttpMessageSigning.Verification {
    /// <inheritdoc />
    public interface ICachingClientStore : IClientStore {
        /// <summary>
        /// Evict the cached <see cref="Client"/> that corresponds to the specified identifier.
        /// </summary>
        /// <param name="clientId">The identifier of the registered client to evict from the cache.</param>
        void Evict(KeyId clientId);
    }
}