using System;
using System.Diagnostics;

namespace Dalion.HttpMessageSigning.Validation {
    /// <summary>
    /// Represents an entry in the list of known clients, that the server maintains.
    /// </summary>
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public class Client : IEquatable<Client> {
        public Client(KeyId id, string secret, SignatureAlgorithm signatureAlgorithm, HashAlgorithm hashAlgorithm, params Claim[] claims) {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("Value cannot be null or empty.", nameof(id));
            if (string.IsNullOrEmpty(secret)) throw new ArgumentException("Value cannot be null or empty.", nameof(secret));
            if (hashAlgorithm == HashAlgorithm.None) throw new ArgumentException("A hash algorithm must be specified.", nameof(hashAlgorithm));
            SignatureAlgorithm = signatureAlgorithm;
            HashAlgorithm = hashAlgorithm;
            Claims = claims ?? Array.Empty<Claim>();
            Id = id;
            Secret = secret;
        }

        /// <summary>
        /// Gets the identity of the client that can be looked up by the server.
        /// </summary>
        public KeyId Id { get; }

        /// <summary>
        /// Gets the secret of the client which can be used to validate the signature.
        /// </summary>
        /// <remarks>For symmetric signature algorithms, this is the shared key. For asymmetric signature algorithms, this value represents the public key of the client.</remarks>
        public string Secret { get; }

        /// <summary>
        /// Gets the algorithm that is used to create the signature.
        /// </summary>
        public SignatureAlgorithm SignatureAlgorithm { get; }
        
        /// <summary>
        /// Gets the algorithm that is used to create the hash value.
        /// </summary>
        public HashAlgorithm HashAlgorithm { get; }

        /// <summary>
        /// Gets the additional claims that the validated principal will have upon successful signature validation.
        /// </summary>
        public Claim[] Claims { get; }

        public bool Equals(Client other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id;
        }

        public override bool Equals(object obj) {
            return ReferenceEquals(this, obj) || obj is Client other && Equals(other);
        }

        public override int GetHashCode() {
            return Id.GetHashCode();
        }

        public override string ToString() {
            return Id;
        }
    }
}