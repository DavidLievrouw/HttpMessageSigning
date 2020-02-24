using System;
using System.Text;

namespace Dalion.HttpMessageSigning.Signing {
    internal class AuthorizationHeaderParamCreator : IAuthorizationHeaderParamCreator {
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

            return sb.ToString();
        }
    }
}