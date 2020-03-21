using System;
using System.Linq;
using System.Security.Cryptography;

namespace Dalion.HttpMessageSigning {
    /// <summary>
    /// Represents settings to be used when signing a http request message.
    /// </summary>
    public class SigningSettings : IValidatable, IDisposable, ICloneable {
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
        /// The ordered list of names of request headers to include when generating the signature for the message.
        /// </summary>
        /// <remarks>When empty, the default headers will be included, according to the spec.</remarks>
        public HeaderName[] Headers { get; set; } = Array.Empty<HeaderName>();

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

            if (Headers != null && Headers.Contains(HeaderName.PredefinedHeaderNames.Created)) {
                throw new PlatformNotSupportedException($"The current platform disallows headers with token separator characters. Disallowed header: {HeaderName.PredefinedHeaderNames.Created}.");
            }
            if (Headers != null && Headers.Contains(HeaderName.PredefinedHeaderNames.Expires)) {
                throw new PlatformNotSupportedException($"The current platform disallows headers with token separator characters. Disallowed header: {HeaderName.PredefinedHeaderNames.Expires}.");
            }
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
                DigestHashAlgorithm = DigestHashAlgorithm
            };
        }
    }
}