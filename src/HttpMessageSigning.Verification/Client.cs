using System;
using System.Diagnostics;
using System.Security.Claims;

namespace Dalion.HttpMessageSigning.Verification {
    /// <summary>
    /// Represents an entry in the list of known clients, that the server maintains.
    /// </summary>
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public class Client : IEquatable<Client>, ICloneable, IDisposable {
        public static readonly TimeSpan DefaultNonceLifetime = TimeSpan.FromMinutes(5);
        
        /// <summary>
        /// Create a new instance of the <see cref="Client"/> class.
        /// </summary>
        /// <param name="id">The identity of the client that can be looked up by the server.</param>
        /// <param name="name">The descriptive name of the client</param>
        /// <param name="signatureAlgorithm">The <see cref="Dalion.HttpMessageSigning.ISignatureAlgorithm"/> that is used to verify the signature.</param>
        /// <param name="claims">The additional claims that the validated principal will have upon successful signature verification.</param>
        [Obsolete("Please use an overload that takes a " + nameof(NonceLifetime) + " argument.")]
        public Client(KeyId id, string name, ISignatureAlgorithm signatureAlgorithm, params Claim[] claims) : this(id, name, signatureAlgorithm, DefaultNonceLifetime, claims) {
        }
        
        /// <summary>
        /// Create a new instance of the <see cref="Client"/> class.
        /// </summary>
        /// <param name="id">The identity of the client that can be looked up by the server.</param>
        /// <param name="name">The descriptive name of the client</param>
        /// <param name="signatureAlgorithm">The <see cref="Dalion.HttpMessageSigning.ISignatureAlgorithm"/> that is used to verify the signature.</param>
        /// <param name="nonceLifetime">The time span after which repeated nonce values are allowed again.</param>
        /// <param name="claims">The additional claims that the validated principal will have upon successful signature verification.</param>
        public Client(KeyId id, string name, ISignatureAlgorithm signatureAlgorithm, TimeSpan nonceLifetime, params Claim[] claims) {
            if (id == KeyId.Empty) throw new ArgumentException("Value cannot be null or empty.", nameof(id));
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Value cannot be null or empty.", nameof(name));
            if (nonceLifetime <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(nonceLifetime), nonceLifetime, "The specified nonce expiration time span is invalid");
            NonceLifetime = nonceLifetime;
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
        /// Gets the <see cref="Dalion.HttpMessageSigning.ISignatureAlgorithm"/> that is used to verify the signature.
        /// </summary>
        public ISignatureAlgorithm SignatureAlgorithm { get; }

        /// <summary>
        /// Gets the time span after which repeated nonce values are allowed again.
        /// </summary>
        public TimeSpan NonceLifetime { get; }
        
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
            return new Client(Id, Name, SignatureAlgorithm, NonceLifetime, (Claim[])Claims.Clone());
        }

        public virtual void Dispose() {
            SignatureAlgorithm?.Dispose();
        }
    }
}