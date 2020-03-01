using System;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification {
    /// <summary>
    /// Represents a store that the server can query to obtain client-specific settings for request signature verification.
    /// </summary>
    public interface IClientStore : IDisposable {
        /// <summary>
        /// Registers a client, and its settings to perform signature verification.
        /// </summary>
        /// <param name="client">The entry that represents a known client.</param>
        Task Register(Client client);
        
        /// <summary>
        /// Gets the registered client that corresponds to the specified identifier.
        /// </summary>
        /// <param name="id">The identifier of the registered client to get.</param>
        /// <returns>The registered client that corresponds to the specified identifier.</returns>
        Task<Client> Get(KeyId id);
    }
}