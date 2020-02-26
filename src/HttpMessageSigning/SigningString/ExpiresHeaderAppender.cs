using System;
using System.Linq;

namespace Dalion.HttpMessageSigning.SigningString {
    internal class ExpiresHeaderAppender : IHeaderAppender {
        private static readonly string[] AlgorithmNamesThatDoNotAllowExpiresHeader = {"rsa", "hmac", "ecdsa"};
        
        private readonly SigningSettings _settings;
        private readonly DateTimeOffset _timeOfComposing;

        public ExpiresHeaderAppender(SigningSettings settings, DateTimeOffset timeOfComposing) {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _timeOfComposing = timeOfComposing;
        }

        public string BuildStringToAppend(HeaderName header) {
            if (AlgorithmNamesThatDoNotAllowExpiresHeader.Contains(_settings.SignatureAlgorithm.Name, StringComparer.OrdinalIgnoreCase)) {
                throw new HttpMessageSigningException($"It is not allowed to include the {HeaderName.PredefinedHeaderNames.Expires} header in the signature, when the signature algorithm is '{_settings.SignatureAlgorithm.Name}'.");
            }
            
            var expiresValue = _timeOfComposing.Add(_settings.Expires).ToUnixTimeSeconds();
            return "\n" + new Header(HeaderName.PredefinedHeaderNames.Expires, expiresValue.ToString());
        }
    }
}