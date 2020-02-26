using System;
using System.Linq;

namespace Dalion.HttpMessageSigning.SigningString {
    internal class CreatedHeaderAppender : IHeaderAppender {
        private static readonly string[] AlgorithmNamesThatDoNotAllowCreatedHeader = {"rsa", "hmac", "ecdsa"};
        
        private readonly HttpRequestForSigning _request;
        private readonly DateTimeOffset _timeOfComposing;

        public CreatedHeaderAppender(HttpRequestForSigning request, DateTimeOffset timeOfComposing) {
            _request = request ?? throw new ArgumentNullException(nameof(request));
            _timeOfComposing = timeOfComposing;
        }

        public string BuildStringToAppend(HeaderName header) {
            if (AlgorithmNamesThatDoNotAllowCreatedHeader.Contains(_request.SignatureAlgorithmName, StringComparer.OrdinalIgnoreCase)) {
                throw new HttpMessageSigningException($"It is not allowed to include the {HeaderName.PredefinedHeaderNames.Created} header in the signature, when the signature algorithm is '{_request.SignatureAlgorithmName}'.");
            }
            
            // ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable
            var createdValue = _timeOfComposing.ToUnixTimeSeconds();
            return "\n" + new Header(HeaderName.PredefinedHeaderNames.Created, createdValue.ToString());
        }
    }
}