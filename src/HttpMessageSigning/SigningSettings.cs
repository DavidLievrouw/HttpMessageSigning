using System;

namespace Dalion.HttpMessageSigning {
    /// <summary>
    /// Represents settings to be used when signing a http request message.
    /// </summary>
    public class SigningSettings : IValidatable {
        /// <summary>
        /// The entity that the server can use to look up the component they need to verify the signature.
        /// </summary>
        public KeyId KeyId { get; set; } = KeyId.Empty;

        /// <summary>
        /// Gets or sets the <see cref="Dalion.HttpMessageSigning.ISignatureAlgorithm"/> that is used to create the signature.
        /// </summary>
        public ISignatureAlgorithm SignatureAlgorithm { get; set; }
        
        /// <summary>
        /// Gets or sets the timespan after which the signature is considered expired.
        /// </summary>
        public TimeSpan Expires { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Gets or sets the hash algorithm to use to generate a Digest http header, to be able to verify that the body has not been modified.
        /// </summary>
        /// <remarks>Select <see cref="Dalion.HttpMessageSigning.HashAlgorithm.None"/> to disable Digest header generation.</remarks>
        public HashAlgorithm DigestHashAlgorithm { get; set; } = HashAlgorithm.SHA256;
        
        /// <summary>
        /// The ordered list of names of request headers to include when generating the signature for the message.
        /// </summary>
        /// <remarks>When empty, the default headers will be included, according to the spec.</remarks>
        public HeaderName[] Headers { get; set; } = Array.Empty<HeaderName>();

        void IValidatable.Validate() {
            Validate();
        }

        internal void Validate() {
            if (KeyId == KeyId.Empty) throw new ValidationException($"The signing settings do not specify a valid {nameof(KeyId)}.");
            if (SignatureAlgorithm == null) throw new ValidationException($"The signing settings do not specify a valid {nameof(SignatureAlgorithm)}.");
            if (Expires <= TimeSpan.Zero) throw new ValidationException($"The signing settings do not specify a valid value for {nameof(Expires)}.");
        }
    }
}