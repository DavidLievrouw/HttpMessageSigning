using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification {
    /// <summary>
    /// Represents an in-memory store that the server can query to obtain client-specific settings for request signature verification.
    /// </summary>
    public class InMemoryClientStore : IClientStore {
        private readonly IList<Client> _entries;

        /// <summary>
        /// Create a new empty instance of the <see cref="InMemoryClientStore"/> class.
        /// </summary>
        public InMemoryClientStore() {
            _entries = new List<Client>();
        }
        
        /// <summary>
        /// Create a new instance of the <see cref="InMemoryClientStore"/> class, containing the specified <see cref="Client"/> instances.
        /// </summary>
        /// <param name="clients">The <see cref="Client"/> instances to register.</param>
        public InMemoryClientStore(params Client[] clients) {
            if (clients == null) clients = Array.Empty<Client>();
            _entries = new List<Client>(clients);
        }
        
        /// <summary>
        /// Registers a <see cref="Client"/>, and its settings to perform <see cref="Signature"/> verification.
        /// </summary>
        /// <param name="client">The entry that represents a known client.</param>
        public Task Register(Client client) {
            if (client == null) throw new ArgumentNullException(nameof(client));

            var idx = _entries.IndexOf(client);
            if (idx < 0) {
                _entries.Add(client);
            }
            else {
                _entries[idx] = client;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets the registered <see cref="Client"/> that corresponds to the specified identifier.
        /// </summary>
        /// <param name="clientId">The identifier of the registered client to get.</param>
        /// <returns>The registered client that corresponds to the specified identifier.</returns>
        public Task<Client> Get(KeyId clientId) {
            if (clientId == KeyId.Empty) throw new ArgumentException("Value cannot be null or empty.", nameof(clientId));
            
            var match = _entries.FirstOrDefault(_ => _.Id == clientId);

            return Task.FromResult(match);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() {
            foreach (var entry in _entries) {
                entry?.Dispose();
            }
            _entries.Clear();
        }
    }
}