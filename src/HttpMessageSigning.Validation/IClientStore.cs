namespace Dalion.HttpMessageSigning.Validation {
    /// <summary>
    /// Represents a store that the server can query to obtain client-specific settings for request signature validation.
    /// </summary>
    public interface IClientStore {
        /// <summary>
        /// Registers a client, and its settings to perform signature validation.
        /// </summary>
        /// <param name="client">The entry that represents a known client.</param>
        void Register(Client client);
        
        /// <summary>
        /// Gets the registered client that corresponds to the specified identifier.
        /// </summary>
        /// <param name="id">The identifier of the registered client to get.</param>
        /// <returns>The registered client that corresponds to the specified identifier.</returns>
        Client Get(string id);
    }
}