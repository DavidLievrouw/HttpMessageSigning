using System;
using System.Linq;

namespace Dalion.HttpMessageSigning.SigningString {
    internal class ExpiresHeaderAppender : IHeaderAppender {
        private static readonly string[] AlgorithmNamesThatDoNotAllowExpiresHeader = {"rsa", "hmac", "ecdsa"};

        private readonly HttpRequestForSigning _requestForSigning;
        private readonly DateTimeOffset _timeOfComposing;
        private readonly TimeSpan _expires;

        public ExpiresHeaderAppender(HttpRequestForSigning requestForSigning, DateTimeOffset timeOfComposing, TimeSpan expires) {
            _requestForSigning = requestForSigning ?? throw new ArgumentNullException(nameof(requestForSigning));
            _timeOfComposing = timeOfComposing;
            _expires = expires;
        }

        public string BuildStringToAppend(HeaderName header) {
            if (AlgorithmNamesThatDoNotAllowExpiresHeader.Contains(_requestForSigning.SignatureAlgorithmName, StringComparer.OrdinalIgnoreCase)) {
                throw new HttpMessageSigningException($"It is not allowed to include the {HeaderName.PredefinedHeaderNames.Expires} header in the signature, when the signature algorithm is '{_requestForSigning.SignatureAlgorithmName}'.");
            }

            var expiresValue = _timeOfComposing.Add(_expires).ToUnixTimeSeconds();
            return "\n" + new Header(HeaderName.PredefinedHeaderNames.Expires, expiresValue.ToString());
        }
    }
}