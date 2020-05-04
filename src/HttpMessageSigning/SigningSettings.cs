using System;
using System.Linq;
using System.Security.Cryptography;

namespace Dalion.HttpMessageSigning {
    /// <summary>
    /// Represents settings to be used when signing a http request message.
    /// </summary>
    public class SigningSettings : IValidatable, IDisposable, ICloneable {
        private static readonly string[] SupportedSignatureAlgorithmNames = {"rsa", "ecdsa", "hmac", "hs2019"};
        
        /// <summary>
        /// The entity that the server can use to look up the component they need to verify the signature.
        /// </summary>
        internal KeyId KeyId { get; set; } = KeyId.Empty;

        /// <summary>
        /// Gets or sets the <see cref="Dalion.HttpMessageSigning.ISignatureAlgorithm"/> that is used to create the signature.
        /// </summary>
        public ISignatureAlgorithm SignatureAlgorithm { get; set; }
        
        /// <summary>
        /// Gets or sets the timespan after which the signature is considered expired.
        /// </summary>
        public TimeSpan Expires { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Gets or sets a value indicating whether a 'Nonce' value will be included in the request signature.
        /// </summary>
        /// <remarks>A 'Nonce' value is used to avoid replay attacks.</remarks>
        public bool EnableNonce { get; set; } = true;
        
        /// <summary>
        /// Gets or sets the hash algorithm to use to generate a Digest http header, to be able to verify that the body has not been modified.
        /// </summary>
        /// <remarks>Set to 'default' to disable Digest header generation.</remarks>
        public HashAlgorithmName DigestHashAlgorithm { get; set; } = default;

        /// <summary>
        /// Gets or sets the ordered list of names of request headers to include when generating the signature for the message.
        /// </summary>
        /// <remarks>When empty, the default headers will be included, according to the spec.</remarks>
        public HeaderName[] Headers { get; set; }

        /// <summary>
        /// Gets or sets the name of the authorization scheme for the authorization header.
        /// </summary>
        public string AuthorizationScheme { get; set; } = "Signature";
        
        /// <summary>
        /// Gets or sets a value indicating whether the 'algorithm' parameter should report deprecated algorithm names, instead of 'hs2019', for backwards compatibility.
        /// </summary>
        /// <remarks>Setting this to 'false' causes the value of the 'algorithm' parameter to be 'hs2019'.</remarks>
        public bool UseDeprecatedAlgorithmParameter { get; set; } = false;
        
        /// <summary>
        /// Gets or sets a value indicating whether to automatically make the headers that are recommended by the spec a part of the signing string.
        /// </summary>
        /// <remarks>To fully conform with the spec, this setting should be set to 'false'.</remarks>
        public bool AutomaticallyAddRecommendedHeaders { get; set; } = true;
        
        /// <summary>
        /// Gets or sets the <see cref="RequestSigningEvents"/> to notify when signing requests.
        /// </summary>
        public RequestSigningEvents Events { get; set; } = new RequestSigningEvents();
        
        void IValidatable.Validate() {
            Validate();
        }

        internal void Validate() {
            if (KeyId == KeyId.Empty) throw new ValidationException($"The signing settings do not specify a valid {nameof(KeyId)}.");
            if (SignatureAlgorithm == null) throw new ValidationException($"The signing settings do not specify a valid {nameof(SignatureAlgorithm)}.");
            if (Expires <= TimeSpan.Zero) throw new ValidationException($"The signing settings do not specify a valid value for {nameof(Expires)}.");
            if (string.IsNullOrEmpty(AuthorizationScheme)) throw new ValidationException($"The signing settings do not specify a valid value for {nameof(AuthorizationScheme)}.");
            if (Headers == null) throw new ValidationException($"{nameof(Headers)} cannot be unspecified (null).");
            if (!Headers.Any()) throw new ValidationException($"{nameof(Headers)} cannot be unspecified empty.");
            if (!SupportedSignatureAlgorithmNames.Contains(SignatureAlgorithm.Name, StringComparer.OrdinalIgnoreCase)) throw new ValidationException($"The specified signature algorithm ({SignatureAlgorithm.Name}) is not supported.");
        }

        public void Dispose() {
            SignatureAlgorithm?.Dispose();
        }

        public object Clone() {
            return new SigningSettings {
                Expires = Expires,
                Headers = (HeaderName[])Headers?.Clone(),
                KeyId = KeyId,
                SignatureAlgorithm = SignatureAlgorithm,
                DigestHashAlgorithm = DigestHashAlgorithm,
                AuthorizationScheme = AuthorizationScheme,
                EnableNonce = EnableNonce,
                UseDeprecatedAlgorithmParameter = UseDeprecatedAlgorithmParameter,
                AutomaticallyAddRecommendedHeaders = AutomaticallyAddRecommendedHeaders,
                Events = new RequestSigningEvents {
                    OnRequestSigned = Events?.OnRequestSigned,
                    OnRequestSigning = Events?.OnRequestSigning,
                    OnSigningStringComposed = Events?.OnSigningStringComposed
                }
            };
        }
    }
}