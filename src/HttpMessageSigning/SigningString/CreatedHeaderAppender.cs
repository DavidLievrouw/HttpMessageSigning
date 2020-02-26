using System;
using System.Linq;

namespace Dalion.HttpMessageSigning.SigningString {
    internal class CreatedHeaderAppender : IHeaderAppender {
        private static readonly string[] AlgorithmNamesThatDoNotAllowCreatedHeader = {"rsa", "hmac", "ecdsa"};
        
        private readonly SigningSettings _settings;
        private readonly DateTimeOffset _timeOfComposing;

        public CreatedHeaderAppender(SigningSettings settings, DateTimeOffset timeOfComposing) {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _timeOfComposing = timeOfComposing;
        }

        public string BuildStringToAppend(HeaderName header) {
            if (AlgorithmNamesThatDoNotAllowCreatedHeader.Contains(_settings.SignatureAlgorithm.Name, StringComparer.OrdinalIgnoreCase)) {
                throw new HttpMessageSigningException($"It is not allowed to include the {HeaderName.PredefinedHeaderNames.Created} header in the signature, when the signature algorithm is '{_settings.SignatureAlgorithm.Name}'.");
            }
            
            // ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable
            var createdValue = _timeOfComposing.ToUnixTimeSeconds();
            return "\n" + new Header(HeaderName.PredefinedHeaderNames.Created, createdValue.ToString());
        }
    }
}