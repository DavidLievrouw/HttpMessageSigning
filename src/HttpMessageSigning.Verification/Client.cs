using System;
using System.Diagnostics;
using System.Security.Claims;

namespace Dalion.HttpMessageSigning.Verification {
    /// <summary>
    /// Represents an entry in the list of known clients, that the server maintains.
    /// </summary>
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public class Client : IEquatable<Client>, ICloneable, IDisposable {
        public Client(KeyId id, string name, ISignatureAlgorithm signatureAlgorithm, params Claim[] claims) {
            if (id == KeyId.Empty) throw new ArgumentException("Value cannot be null or empty.", nameof(id));
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Value cannot be null or empty.", nameof(name));
            Claims = claims ?? Array.Empty<Claim>();
            Id = id;
            Name = name;
            SignatureAlgorithm = signatureAlgorithm ?? throw new ArgumentNullException(nameof(signatureAlgorithm));
        }

        /// <summary>
        /// Gets the identity of the client that can be looked up by the server.
        /// </summary>
        public KeyId Id { get; }
        
        /// <summary>
        /// Gets the descriptive name of the client.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or sets the <see cref="Dalion.HttpMessageSigning.ISignatureAlgorithm"/> that is used to verify the signature.
        /// </summary>
        public ISignatureAlgorithm SignatureAlgorithm { get; }

        /// <summary>
        /// Gets the additional claims that the validated principal will have upon successful signature verification.
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

        public object Clone() {
            return new Client(Id, Name, SignatureAlgorithm, (Claim[])Claims.Clone());
        }

        public virtual void Dispose() {
            SignatureAlgorithm?.Dispose();
        }
    }
}