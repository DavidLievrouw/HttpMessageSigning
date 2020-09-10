using System;
using System.Diagnostics;
using System.Security.Claims;

namespace Dalion.HttpMessageSigning.Verification {
    /// <summary>
    ///     Represents an entry in the list of known clients, that the server maintains.
    /// </summary>
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public class Client : IEquatable<Client>, ICloneable, IDisposable {        
        /// <summary>
        ///     Represents the default time span after which repeated nonce values are allowed again.
        /// </summary>
        [Obsolete("Please use the " + nameof(ClientOptions.DefaultNonceLifetime) + " property from the " + nameof(ClientOptions) + " class.")]
        public static readonly TimeSpan DefaultNonceLifetime = ClientOptions.DefaultNonceLifetime;

        /// <summary>
        ///     Represents the default clock skew to allow when validation a time.
        /// </summary>
        [Obsolete("Please use the " + nameof(ClientOptions.DefaultClockSkew) + " property from the " + nameof(ClientOptions) + " class.")]
        public static readonly TimeSpan DefaultClockSkew = ClientOptions.DefaultClockSkew;
        
        private readonly ClientOptions _options;

        /// <summary>
        ///     Create a new instance of the <see cref="Client" /> class.
        /// </summary>
        /// <param name="id">The identity of the client that can be looked up by the server.</param>
        /// <param name="name">The descriptive name of the client</param>
        /// <param name="signatureAlgorithm">The <see cref="Dalion.HttpMessageSigning.ISignatureAlgorithm" /> that is used to verify the signature.</param>
        /// <param name="claims">The additional claims that the validated principal will have upon successful signature verification.</param>
        [Obsolete("Please use an overload that takes a " + nameof(ClientOptions) + " argument.")]
        public Client(KeyId id, string name, ISignatureAlgorithm signatureAlgorithm, params Claim[] claims)
            : this(id, name, signatureAlgorithm, new ClientOptions {Claims = claims}) { }

        /// <summary>
        ///     Create a new instance of the <see cref="Client" /> class.
        /// </summary>
        /// <param name="id">The identity of the client that can be looked up by the server.</param>
        /// <param name="name">The descriptive name of the client</param>
        /// <param name="signatureAlgorithm">The <see cref="Dalion.HttpMessageSigning.ISignatureAlgorithm" /> that is used to verify the signature.</param>
        /// <param name="nonceLifetime">The time span after which repeated nonce values are allowed again.</param>
        /// <param name="claims">The additional claims that the validated principal will have upon successful signature verification.</param>
        [Obsolete("Please use an overload that takes a " + nameof(ClientOptions) + " argument.")]
        public Client(KeyId id, string name, ISignatureAlgorithm signatureAlgorithm, TimeSpan nonceLifetime, params Claim[] claims)
            : this(id, name, signatureAlgorithm, new ClientOptions {NonceLifetime = nonceLifetime, Claims = claims}) { }

        /// <summary>
        ///     Create a new instance of the <see cref="Client" /> class.
        /// </summary>
        /// <param name="id">The identity of the client that can be looked up by the server.</param>
        /// <param name="name">The descriptive name of the client</param>
        /// <param name="signatureAlgorithm">The <see cref="Dalion.HttpMessageSigning.ISignatureAlgorithm" /> that is used to verify the signature.</param>
        /// <param name="nonceLifetime">The time span after which repeated nonce values are allowed again.</param>
        /// <param name="clockSkew">The clock skew to allow when validation a time.</param>
        /// <param name="claims">The additional claims that the validated principal will have upon successful signature verification.</param>
        [Obsolete("Please use an overload that takes a " + nameof(ClientOptions) + " argument.")]
        public Client(KeyId id, string name, ISignatureAlgorithm signatureAlgorithm, TimeSpan nonceLifetime, TimeSpan clockSkew, params Claim[] claims)
            : this(id, name, signatureAlgorithm, new ClientOptions {NonceLifetime = nonceLifetime, ClockSkew = clockSkew, Claims = claims}) { }

        /// <summary>
        ///     Create a new instance of the <see cref="Client" /> class.
        /// </summary>
        /// <param name="id">The identity of the client that can be looked up by the server.</param>
        /// <param name="name">The descriptive name of the client</param>
        /// <param name="signatureAlgorithm">The <see cref="Dalion.HttpMessageSigning.ISignatureAlgorithm" /> that is used to verify the signature.</param>
        /// <param name="nonceLifetime">The time span after which repeated nonce values are allowed again.</param>
        /// <param name="clockSkew">The clock skew to allow when validation a time.</param>
        /// <param name="claims">The additional claims that the validated principal will have upon successful signature verification.</param>
        /// <param name="requestTargetEscaping">The method of escaping the value of the (request-target) pseudo-header.</param>
        public Client(
            KeyId id,
            string name,
            ISignatureAlgorithm signatureAlgorithm,
            TimeSpan nonceLifetime,
            TimeSpan clockSkew,
            RequestTargetEscaping requestTargetEscaping,
            params Claim[] claims)
            : this(id, name, signatureAlgorithm, new ClientOptions {
                RequestTargetEscaping = requestTargetEscaping,
                NonceLifetime = nonceLifetime,
                ClockSkew = clockSkew,
                Claims = claims
            }) { }

        /// <summary>
        ///     Create a new instance of the <see cref="Client" /> class.
        /// </summary>
        /// <param name="id">The identity of the client that can be looked up by the server.</param>
        /// <param name="name">The descriptive name of the client</param>
        /// <param name="signatureAlgorithm">The <see cref="Dalion.HttpMessageSigning.ISignatureAlgorithm" /> that is used to verify the signature.</param>
        /// <param name="options">The options to apply during creation of the new <see cref="Client" /> instance.</param>
        public Client(KeyId id, string name, ISignatureAlgorithm signatureAlgorithm, ClientOptions options) {
            if (id == KeyId.Empty) throw new ArgumentException("Value cannot be null or empty.", nameof(id));
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Value cannot be null or empty.", nameof(name));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _options.Validate();
            Id = id;
            Name = name;
            SignatureAlgorithm = signatureAlgorithm ?? throw new ArgumentNullException(nameof(signatureAlgorithm));
        }

        /// <summary>
        ///     Gets the identity of the client that can be looked up by the server.
        /// </summary>
        public KeyId Id { get; }

        /// <summary>
        ///     Gets the descriptive name of the client.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets the <see cref="Dalion.HttpMessageSigning.ISignatureAlgorithm" /> that is used to verify the signature.
        /// </summary>
        public ISignatureAlgorithm SignatureAlgorithm { get; }

        /// <summary>
        ///     Gets the time span after which repeated nonce values are allowed again.
        /// </summary>
        public TimeSpan NonceLifetime => _options.NonceLifetime;

        /// <summary>
        ///     Gets the clock skew to allow when validation a time.
        /// </summary>
        public TimeSpan ClockSkew => _options.ClockSkew;

        /// <summary>
        ///     Gets the method of escaping the value of the (request-target) pseudo-header.
        /// </summary>
        public RequestTargetEscaping RequestTargetEscaping => _options.RequestTargetEscaping;

        /// <summary>
        ///     Gets the additional claims that the validated principal will have upon successful signature verification.
        /// </summary>
        public Claim[] Claims => _options.Claims ?? Array.Empty<Claim>();

        /// <inheritdoc />
        public object Clone() {
            return new Client(Id, Name, SignatureAlgorithm, NonceLifetime, ClockSkew, RequestTargetEscaping, (Claim[]) Claims.Clone());
        }

        /// <inheritdoc />
        public virtual void Dispose() {
            SignatureAlgorithm?.Dispose();
        }

        /// <inheritdoc />
        public bool Equals(Client other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id;
        }

        /// <inheritdoc />
        public override bool Equals(object obj) {
            return ReferenceEquals(this, obj) || obj is Client other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode() {
            return Id.GetHashCode();
        }

        /// <inheritdoc />
        public override string ToString() {
            return Id;
        }
    }
}