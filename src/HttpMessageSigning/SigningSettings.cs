using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Dalion.HttpMessageSigning {
    /// <summary>
    ///     Represents settings to be used when signing a http request message.
    /// </summary>
    public class SigningSettings : IValidatable, IDisposable, ICloneable {
        private static readonly string[] SupportedSignatureAlgorithmNames = {"rsa", "ecdsa", "hmac", "hs2019"};

        /// <summary>
        ///     The entity that the server can use to look up the component they need to verify the signature.
        /// </summary>
        internal KeyId KeyId { get; set; } = KeyId.Empty;

        /// <summary>
        ///     Gets or sets the <see cref="Dalion.HttpMessageSigning.ISignatureAlgorithm" /> that is used to create the signature.
        /// </summary>
        public ISignatureAlgorithm SignatureAlgorithm { get; set; }

        /// <summary>
        ///     Gets or sets the timespan after which the signature is considered expired.
        /// </summary>
        public TimeSpan Expires { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>
        ///     Gets or sets a value indicating whether a 'Nonce' value will be included in the request signature.
        /// </summary>
        /// <remarks>A 'Nonce' value is used to avoid replay attacks.</remarks>
        public bool EnableNonce { get; set; } = true;

        /// <summary>
        ///     Gets or sets the hash algorithm to use to generate a Digest http header, to be able to verify that the body has not been modified.
        /// </summary>
        /// <remarks>Set to 'default' to disable Digest header generation.</remarks>
        public HashAlgorithmName DigestHashAlgorithm { get; set; } = default;

        /// <summary>
        ///     Gets or sets the ordered list of names of request headers to include when generating the signature for the message.
        /// </summary>
        /// <remarks>When empty, the default headers will be included, according to the spec.</remarks>
        public HeaderName[] Headers { get; set; }

        /// <summary>
        ///     Gets or sets the name of the authorization scheme for the authorization header.
        /// </summary>
        public string AuthorizationScheme { get; set; } = "Signature";

        /// <summary>
        ///     Gets or sets a value indicating whether the 'algorithm' parameter should report deprecated algorithm names, instead of 'hs2019', for backwards compatibility.
        /// </summary>
        /// <remarks>Setting this to 'false' causes the value of the 'algorithm' parameter to be 'hs2019'.</remarks>
        public bool UseDeprecatedAlgorithmParameter { get; set; } = false;

        /// <summary>
        ///     Gets or sets a value indicating whether to automatically make the headers that are recommended by the spec a part of the signing string.
        /// </summary>
        /// <remarks>To fully conform with the spec, this setting should be set to 'false'.</remarks>
        public bool AutomaticallyAddRecommendedHeaders { get; set; } = true;

        /// <summary>
        ///     Gets or sets the method of escaping the value of the (request-target) pseudo-header.
        /// </summary>
        public RequestTargetEscaping RequestTargetEscaping { get; set; } = RequestTargetEscaping.RFC3986;

        /// <summary>
        ///     Gets or sets the <see cref="RequestSigningEvents" /> to notify when signing requests.
        /// </summary>
        public RequestSigningEvents Events { get; set; } = new RequestSigningEvents();

        /// <inheritdoc />
        public object Clone() {
            return new SigningSettings {
                Expires = Expires,
                Headers = (HeaderName[]) Headers?.Clone(),
                KeyId = KeyId,
                SignatureAlgorithm = SignatureAlgorithm,
                DigestHashAlgorithm = DigestHashAlgorithm,
                AuthorizationScheme = AuthorizationScheme,
                EnableNonce = EnableNonce,
                UseDeprecatedAlgorithmParameter = UseDeprecatedAlgorithmParameter,
                AutomaticallyAddRecommendedHeaders = AutomaticallyAddRecommendedHeaders,
                RequestTargetEscaping = RequestTargetEscaping,
                Events = new RequestSigningEvents {
                    OnRequestSigned = Events?.OnRequestSigned,
                    OnRequestSigning = Events?.OnRequestSigning,
                    OnSigningStringComposed = Events?.OnSigningStringComposed
                }
            };
        }

        /// <inheritdoc />
        public void Dispose() {
            SignatureAlgorithm?.Dispose();
        }

        /// <inheritdoc />
        public void Validate() {
            var error = GetValidationErrors().FirstOrDefault();
            if (error != null) throw new ValidationException(error.Message);
        }

        /// <inheritdoc />
        public IEnumerable<ValidationError> GetValidationErrors() {
            var errors = new List<ValidationError>();
            if (KeyId == KeyId.Empty) errors.Add(new ValidationError(nameof(KeyId), $"The signing settings do not specify a valid {nameof(KeyId)}."));
            if (SignatureAlgorithm == null) errors.Add(new ValidationError(nameof(SignatureAlgorithm), $"The signing settings do not specify a valid {nameof(SignatureAlgorithm)}."));
            if (SignatureAlgorithm != null && !SupportedSignatureAlgorithmNames.Contains(SignatureAlgorithm.Name, StringComparer.OrdinalIgnoreCase)) errors.Add(new ValidationError(nameof(SignatureAlgorithm), $"The specified signature algorithm ({SignatureAlgorithm.Name}) is not supported."));
            if (Expires <= TimeSpan.Zero) errors.Add(new ValidationError(nameof(Expires), $"The signing settings do not specify a valid value for {nameof(Expires)}."));
            if (string.IsNullOrEmpty(AuthorizationScheme)) errors.Add(new ValidationError(nameof(AuthorizationScheme), $"The signing settings do not specify a valid value for {nameof(AuthorizationScheme)}."));
            if (Headers == null) errors.Add(new ValidationError(nameof(Headers), $"{nameof(Headers)} cannot be unspecified (null)."));
            if (Headers != null && !Headers.Any()) errors.Add(new ValidationError(nameof(Headers), $"{nameof(Headers)} cannot be unspecified empty."));
            return errors;
        }
    }
}