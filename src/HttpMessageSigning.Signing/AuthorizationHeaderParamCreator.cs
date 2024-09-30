using System;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Dalion.HttpMessageSigning.Signing {
    internal class AuthorizationHeaderParamCreator : IAuthorizationHeaderParamCreator {
        private readonly ILogger<AuthorizationHeaderParamCreator> _logger;

        public AuthorizationHeaderParamCreator(ILogger<AuthorizationHeaderParamCreator> logger = null) {
            _logger = logger;
        }

        public string CreateParam(Signature signature) {
            if (signature == null) throw new ArgumentNullException(nameof(signature));

            var sb = new StringBuilder();
            sb.Append("keyId=\"")
                .Append(signature.KeyId.Value)
                .Append("\"");

            if (!string.IsNullOrEmpty(signature.Algorithm)) {
                sb.Append(",algorithm=\"").Append(signature.Algorithm).Append("\"");
            }

            if (signature.Created.HasValue) {
                sb.Append(",created=").Append(signature.Created.Value.ToUnixTimeSeconds());
            }

            if (signature.Expires.HasValue) {
                sb.Append(",expires=").Append(signature.Expires.Value.ToUnixTimeSeconds());
            }

            if (signature.Headers != null && signature.Headers.Length > 0) {
                sb.Append(",headers=\"").Append(string.Join(" ", signature.Headers)).Append("\"");
            }

            if (!string.IsNullOrEmpty(signature.Nonce)) {
                sb.Append(",nonce=\"").Append(signature.Nonce).Append("\"");
            }

            sb.Append(",signature=\"").Append(signature.String).Append("\"");

            var param = sb.ToString();

            _logger?.LogDebug("Created the following authorization header param value: {0}", param);

            return param;
        }
    }
}