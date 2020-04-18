using System;
using System.Linq;

namespace Dalion.HttpMessageSigning.SigningString {
    internal class ExpiresHeaderAppender : IHeaderAppender {
        private static readonly string[] AlgorithmNamesThatDoNotAllowExpiresHeader = {"rsa", "hmac", "ecdsa"};

        private readonly string _signatureAlgorithmName;
        private readonly DateTimeOffset _timeOfComposing;
        private readonly TimeSpan _expires;

        public ExpiresHeaderAppender(string signatureAlgorithmName, DateTimeOffset timeOfComposing, TimeSpan expires) {
            _signatureAlgorithmName = signatureAlgorithmName ?? throw new ArgumentNullException(nameof(signatureAlgorithmName));
            _timeOfComposing = timeOfComposing;
            _expires = expires;
        }

        public string BuildStringToAppend(HeaderName header) {
            if (AlgorithmNamesThatDoNotAllowExpiresHeader.Contains(_signatureAlgorithmName, StringComparer.OrdinalIgnoreCase)) {
                throw new HttpMessageSigningException($"It is not allowed to include the {HeaderName.PredefinedHeaderNames.Expires} header in the signature, when the signature algorithm is '{_signatureAlgorithmName}'.");
            }

            var expiresValue = _timeOfComposing.Add(_expires).ToUnixTimeSeconds();
            return "\n" + new Header(HeaderName.PredefinedHeaderNames.Expires, expiresValue.ToString());
        }
    }
}