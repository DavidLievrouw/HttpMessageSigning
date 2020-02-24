using System;
using System.Diagnostics;

namespace Dalion.HttpMessageSigning.Validation {
    /// <summary>
    /// Represents an entry in the list of known clients, that the server maintains.
    /// </summary>
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public class KeyStoreEntry : IEquatable<KeyStoreEntry> {
        public KeyStoreEntry(string id, SignatureAlgorithm signatureAlgorithm, HashAlgorithm hashAlgorithm) {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("Value cannot be null or empty.", nameof(id));
            SignatureAlgorithm = signatureAlgorithm;
            HashAlgorithm = hashAlgorithm;
            Id = id;
        }

        /// <summary>
        /// Gets the identity of the key store entry that can be looked up by the server.
        /// </summary>
        public string Id { get; }
        
        /// <summary>
        /// Gets the algorithm that is used to create the hash value.
        /// </summary>
        public SignatureAlgorithm SignatureAlgorithm { get; }
        
        /// <summary>
        /// Gets the keyed hash algorithm that is used to create the hash value.
        /// </summary>
        public HashAlgorithm HashAlgorithm { get; }

        public bool Equals(KeyStoreEntry other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id;
        }

        public override bool Equals(object obj) {
            return ReferenceEquals(this, obj) || obj is KeyStoreEntry other && Equals(other);
        }

        public override int GetHashCode() {
            return Id.GetHashCode();
        }

        public override string ToString() {
            return Id;
        }
    }
}