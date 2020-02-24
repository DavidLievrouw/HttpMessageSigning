using System;

namespace Dalion.HttpMessageSigning {
    public class SigningSettings : IValidatable {
        public IKeyId KeyId { get; set; }
        public Algorithm Algorithm { get; set; }
        public TimeSpan Expires { get; set; }
        public HeaderName[] Headers { get; set; } = Array.Empty<HeaderName>();
        
        public void Validate() {
            if (KeyId == null) throw new HttpMessageSigningValidationException($"The signing settings do not specify a valid {nameof(KeyId)}.");
            if (Headers == null) throw new HttpMessageSigningValidationException($"The signing settings do not specify valid {nameof(Headers)}.");
            if (Expires <= TimeSpan.Zero) throw new HttpMessageSigningValidationException($"The signing settings do not specify a valid value for {nameof(Expires)}.");
        }
    }
}