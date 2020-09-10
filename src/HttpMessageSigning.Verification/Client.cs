using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Dalion.HttpMessageSigning.Verification {
    /// <summary>
    ///     Represents an entry in the list of known clients, that the server maintains.
    /// </summary>
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public class Client : IEquatable<Client>, ICloneable, IDisposable {
        /// <summary>
        ///     Represents the default time span after which repeated nonce values are allowed again.
        /// </summary>
        [Obsolete("Please use the " + nameof(ClientOptions) + "." + nameof(ClientOptions.Default) + "." + nameof(ClientOptions.Default.NonceLifetime) + " property instead.")]
        public static readonly TimeSpan DefaultNonceLifetime = ClientOptions.Default.NonceLifetime;

        /// <summary>
        ///     Represents the default clock skew to allow when validation a time.
        /// </summary>
        [Obsolete("Please use the " + nameof(ClientOptions) + "." + nameof(ClientOptions.Default) + "." + nameof(ClientOptions.Default.ClockSkew) + " property instead.")]
        public static readonly TimeSpan DefaultClockSkew = ClientOptions.Default.ClockSkew;

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
        public void Dispose() {
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

        /// <summary>
        ///     Create a new instance of the <see cref="Client" /> class, with the specified options.
        /// </summary>
        /// <param name="id">The identity of the client that can be looked up by the server.</param>
        /// <param name="name">The descriptive name of the client.</param>
        /// <param name="signatureAlgorithm">The <see cref="Dalion.HttpMessageSigning.ISignatureAlgorithm" /> that is used to verify the signature.</param>
        /// <param name="configure">A delegate that allows configuring additional options for the new instance. Pass <see langword="null" /> to use the default options.</param>
        /// <returns>The newly created <see cref="Client" /> instance.</returns>
        public static Client Create(KeyId id, string name, ISignatureAlgorithm signatureAlgorithm, Action<ClientOptions> configure = null) {
            var options = new ClientOptions();
            configure?.Invoke(options);

            return new Client(id, name, signatureAlgorithm, options);
        }

        /// <summary>
        ///     Create a new instance of the <see cref="Client" /> class, with the specified options.
        /// </summary>
        /// <param name="id">The identity of the client that can be looked up by the server.</param>
        /// <param name="name">The descriptive name of the client.</param>
        /// <param name="hmacSecret">The shared secret for the HMAC algorithm.</param>
        /// <param name="hashAlgorithm">The name of the hash algorithm to use. Pass <see langword="default" /> to use the default.</param>
        /// <param name="configure">A delegate that allows configuring additional options for the new instance. Pass <see langword="null" /> to use the default options.</param>
        /// <returns>The newly created <see cref="Client" /> instance.</returns>
        public static Client Create(KeyId id, string name, string hmacSecret, HashAlgorithmName hashAlgorithm = default, Action<ClientOptions> configure = null) {
            var signatureAlgorithm = HttpMessageSigning.SignatureAlgorithm.CreateForVerification(
                hmacSecret,
                hashAlgorithm == default ? HashAlgorithmName.SHA512 : hashAlgorithm);

            return Create(id, name, signatureAlgorithm, configure);
        }

        /// <summary>
        ///     Create a new instance of the <see cref="Client" /> class, with the specified options.
        /// </summary>
        /// <param name="id">The identity of the client that can be looked up by the server.</param>
        /// <param name="name">The descriptive name of the client.</param>
        /// <param name="publicParameters">The public RSA algorithm parameters to use.</param>
        /// <param name="hashAlgorithm">The name of the hash algorithm to use. Pass <see langword="default" /> to use the default.</param>
        /// <param name="configure">A delegate that allows configuring additional options for the new instance. Pass <see langword="null" /> to use the default options.</param>
        /// <returns>The newly created <see cref="Client" /> instance.</returns>
        public static Client Create(KeyId id, string name, RSAParameters publicParameters, HashAlgorithmName hashAlgorithm = default, Action<ClientOptions> configure = null) {
            if (EqualityComparer<RSAParameters>.Default.Equals(publicParameters, default)) throw new ArgumentNullException(nameof(publicParameters));

            var signatureAlgorithm = HttpMessageSigning.SignatureAlgorithm.CreateForVerification(
                publicParameters,
                hashAlgorithm == default ? HashAlgorithmName.SHA512 : hashAlgorithm);

            return Create(id, name, signatureAlgorithm, configure);
        }

        /// <summary>
        ///     Create a new instance of the <see cref="Client" /> class, with the specified options.
        /// </summary>
        /// <param name="id">The identity of the client that can be looked up by the server.</param>
        /// <param name="name">The descriptive name of the client.</param>
        /// <param name="publicParameters">The public ECDsa algorithm parameters to use.</param>
        /// <param name="hashAlgorithm">The name of the hash algorithm to use. Pass <see langword="default" /> to use the default.</param>
        /// <param name="configure">A delegate that allows configuring additional options for the new instance. Pass <see langword="null" /> to use the default options.</param>
        /// <returns>The newly created <see cref="Client" /> instance.</returns>
        public static Client Create(KeyId id, string name, ECParameters publicParameters, HashAlgorithmName hashAlgorithm = default, Action<ClientOptions> configure = null) {
            if (EqualityComparer<ECParameters>.Default.Equals(publicParameters, default)) throw new ArgumentNullException(nameof(publicParameters));

            var signatureAlgorithm = HttpMessageSigning.SignatureAlgorithm.CreateForVerification(
                publicParameters,
                hashAlgorithm == default ? HashAlgorithmName.SHA512 : hashAlgorithm);

            return Create(id, name, signatureAlgorithm, configure);
        }
    }
}