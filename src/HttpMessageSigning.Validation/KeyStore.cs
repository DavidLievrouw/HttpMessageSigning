using System;
using System.Collections.Generic;
using System.Linq;

namespace Dalion.HttpMessageSigning.Validation {
    internal class KeyStore : IKeyStore {
        private readonly List<KeyStoreEntry> _entries;

        public KeyStore() {
            _entries = new List<KeyStoreEntry>();
        }

        public void Register(KeyStoreEntry entry) {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            
            if (_entries.Contains(entry)) throw new InvalidOperationException($"An key store entry with id '{entry.Id}' is already registered.");
            
            _entries.Add(entry);
        }

        public KeyStoreEntry Get(string id) {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("Value cannot be null or empty.", nameof(id));
            
            var match = _entries.FirstOrDefault(_ => _.Id == id);

            if (match == null) throw new HttpMessageSigningSignatureValidationException($"No key store entries with id '{id}' are registered with the server.");

            return match;
        }
    }
}