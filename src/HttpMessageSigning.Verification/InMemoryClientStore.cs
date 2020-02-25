using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification {
    /// <summary>
    /// Represents an in-memory store that the server can query to obtain client-specific settings for request signature verification.
    /// </summary>
    public class InMemoryClientStore : IClientStore {
        private readonly List<Client> _entries;

        public InMemoryClientStore() {
            _entries = new List<Client>();
        }

        /// <summary>
        /// Registers a client, and its settings to perform signature verification.
        /// </summary>
        /// <param name="client">The entry that represents a known client.</param>
        public Task Register(Client client) {
            if (client == null) throw new ArgumentNullException(nameof(client));
            
            if (_entries.Contains(client)) throw new InvalidOperationException($"A {nameof(Client)} with id '{client.Id}' is already registered in the server store.");
            
            _entries.Add(client);
            
            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets the registered client that corresponds to the specified identifier.
        /// </summary>
        /// <param name="id">The identifier of the registered client to get.</param>
        /// <returns>The registered client that corresponds to the specified identifier.</returns>
        public Task<Client> Get(string id) {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("Value cannot be null or empty.", nameof(id));
            
            var match = _entries.FirstOrDefault(_ => _.Id == id);

            if (match == null) throw new SignatureVerificationException($"No {nameof(Client)}s with id '{id}' are registered in the server store.");

            return Task.FromResult(match);
        }
    }
}