using System;
using System.Linq;

namespace Dalion.HttpMessageSigning.SigningString {
    internal class CreatedHeaderAppender : IHeaderAppender {
        private static readonly string[] AlgorithmNamesThatDoNotAllowCreatedHeader = {"rsa", "hmac", "ecdsa"};
        
        private readonly string _signatureAlgorithmName;
        private readonly DateTimeOffset _timeOfComposing;

        public CreatedHeaderAppender(string signatureAlgorithmName, DateTimeOffset timeOfComposing) {
            _signatureAlgorithmName = signatureAlgorithmName ?? throw new ArgumentNullException(nameof(signatureAlgorithmName));
            _timeOfComposing = timeOfComposing;
        }

        public string BuildStringToAppend(HeaderName header) {
            if (AlgorithmNamesThatDoNotAllowCreatedHeader.Contains(_signatureAlgorithmName, StringComparer.OrdinalIgnoreCase)) {
                throw new HttpMessageSigningException($"It is not allowed to include the {HeaderName.PredefinedHeaderNames.Created} header in the signature, when the signature algorithm is '{_signatureAlgorithmName}'.");
            }
            
            // ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable
            var createdValue = _timeOfComposing.ToUnixTimeSeconds();
            return "\n" + new Header(HeaderName.PredefinedHeaderNames.Created, createdValue.ToString());
        }
    }
}