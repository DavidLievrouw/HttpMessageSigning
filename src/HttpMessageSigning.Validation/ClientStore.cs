using System;
using System.Collections.Generic;
using System.Linq;

namespace Dalion.HttpMessageSigning.Validation {
    internal class ClientStore : IClientStore {
        private readonly List<Client> _entries;

        public ClientStore() {
            _entries = new List<Client>();
        }

        public void Register(Client client) {
            if (client == null) throw new ArgumentNullException(nameof(client));
            
            if (_entries.Contains(client)) throw new InvalidOperationException($"An key store entry with id '{client.Id}' is already registered.");
            
            _entries.Add(client);
        }

        public Client Get(string id) {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("Value cannot be null or empty.", nameof(id));
            
            var match = _entries.FirstOrDefault(_ => _.Id == id);

            if (match == null) throw new SignatureValidationException($"No key store entries with id '{id}' are registered with the server.");

            return match;
        }
    }
}