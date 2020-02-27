using System;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Dalion.HttpMessageSigning.Signing {
    internal class AuthorizationHeaderParamCreator : IAuthorizationHeaderParamCreator {
        private readonly ILogger<AuthorizationHeaderParamCreator> _logger;
        
        public AuthorizationHeaderParamCreator(ILogger<AuthorizationHeaderParamCreator> logger) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string CreateParam(Signature signature) {
            if (signature == null) throw new ArgumentNullException(nameof(signature));

            var keyId = signature.KeyId.Value;
            var created = signature.Created?.ToUnixTimeSeconds().ToString();
            var expires = signature.Expires?.ToUnixTimeSeconds().ToString();
            var headers = signature.Headers != null
                ? string.Join(" ", signature.Headers)
                : null;

            var sb = new StringBuilder();
            sb.Append("keyId=\"" + keyId + "\"");
            if (!string.IsNullOrEmpty(signature.Algorithm)) sb.Append(",algorithm=\"" + signature.Algorithm + "\"");
            if (!string.IsNullOrEmpty(created)) sb.Append(",created=" + created);
            if (!string.IsNullOrEmpty(expires)) sb.Append(",expires=" + expires);
            if (!string.IsNullOrEmpty(headers)) sb.Append(",headers=\"" + headers + "\"");
            sb.Append(",signature=\"" + signature.String + "\"");

            var param = sb.ToString();

            _logger.LogDebug("Created the following authorization header param value: {0}", param);
            
            return param;
        }
    }
}