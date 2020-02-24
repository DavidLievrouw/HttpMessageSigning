using System;
using System.Text;
using Dalion.HttpMessageSigning.Logging;

namespace Dalion.HttpMessageSigning.Signing {
    internal class AuthorizationHeaderParamCreator : IAuthorizationHeaderParamCreator {
        private readonly IHttpMessageSigningLogger<AuthorizationHeaderParamCreator> _logger;
        
        public AuthorizationHeaderParamCreator(IHttpMessageSigningLogger<AuthorizationHeaderParamCreator> logger) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string CreateParam(Signature signature) {
            if (signature == null) throw new ArgumentNullException(nameof(signature));

            string keyId = (KeyId) signature.KeyId;
            var created = signature.Created?.ToUnixTimeSeconds().ToString();
            var expires = signature.Expires?.ToUnixTimeSeconds().ToString();
            var headers = signature.Headers != null
                ? string.Join(" ", signature.Headers)
                : null;
            var algorithm = signature.SignatureAlgorithm.HasValue && signature.HashAlgorithm.HasValue
                ? $"{signature.SignatureAlgorithm.ToString().ToLowerInvariant()}_{signature.HashAlgorithm.ToString().ToLowerInvariant()}"
                : null;

            var sb = new StringBuilder();
            sb.Append("keyId=\"" + keyId + "\"");
            if (!string.IsNullOrEmpty(algorithm)) sb.Append(",algorithm=\"" + algorithm + "\"");
            if (!string.IsNullOrEmpty(created)) sb.Append(",created=" + created);
            if (!string.IsNullOrEmpty(expires)) sb.Append(",expires=" + expires);
            if (!string.IsNullOrEmpty(headers)) sb.Append(",headers=\"" + headers + "\"");
            sb.Append(",signature=\"" + signature.String + "\"");

            var param = sb.ToString();

            _logger.Debug("Created the following authorization header param value: {0}", param);
            
            return param;
        }
    }
}