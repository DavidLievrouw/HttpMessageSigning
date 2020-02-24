using System;

namespace Dalion.HttpMessageSigning {
    /// <summary>
    /// Represents settings to be used when signing a http request message.
    /// </summary>
    public class SigningSettings : IValidatable {
        /// <summary>
        /// The entity that the server can use to look up the component they need to validate the signature.
        /// </summary>
        public IKeyId KeyId { get; set; }
        
        /// <summary>
        /// The timespan after which the signature is considered expired.
        /// </summary>
        public TimeSpan Expires { get; set; }
        
        /// <summary>
        /// The ordered list of names of request headers to include when generating the signature for the message.
        /// </summary>
        /// <remarks>When empty, the default headers will be included, according to the spec.</remarks>
        public HeaderName[] Headers { get; set; } = Array.Empty<HeaderName>();

        void IValidatable.Validate() {
            Validate();
        }

        internal void Validate() {
            if (KeyId == null) throw new HttpMessageSigningValidationException($"The signing settings do not specify a valid {nameof(KeyId)}.");
            if (Headers == null) throw new HttpMessageSigningValidationException($"The signing settings do not specify valid {nameof(Headers)}.");
            if (Expires <= TimeSpan.Zero) throw new HttpMessageSigningValidationException($"The signing settings do not specify a valid value for {nameof(Expires)}.");
        }
    }
}