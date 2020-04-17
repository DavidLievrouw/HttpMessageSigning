using System;
using System.Linq;
using System.Net.Http;

namespace Dalion.HttpMessageSigning.Signing {
    internal class SigningSettingsSanitizer : ISigningSettingsSanitizer {
        public void SanitizeHeaderNamesToInclude(SigningSettings signingSettings, HttpRequestMessage request) {
            if (signingSettings == null) throw new ArgumentNullException(nameof(signingSettings));
            if (request == null) throw new ArgumentNullException(nameof(request));

            // According to the spec, the header (request-target) should always be a part of the signature string.
            if (!signingSettings.Headers.Contains(HeaderName.PredefinedHeaderNames.RequestTarget)) {
                signingSettings.Headers = AppendHeaderName(signingSettings.Headers, HeaderName.PredefinedHeaderNames.RequestTarget);
            }

            // According to the spec, when the algorithm starts with 'rsa', 'hmac' or 'ecdsa', the Date header should be part of the signature string.
            if (signingSettings.SignatureAlgorithm.ShouldIncludeDateHeader() && !signingSettings.Headers.Contains(HeaderName.PredefinedHeaderNames.Date)) {
                signingSettings.Headers = AppendHeaderName(signingSettings.Headers, HeaderName.PredefinedHeaderNames.Date);
            }

            // According to the spec, when the algorithm does not start with 'rsa', 'hmac' or 'ecdsa', the (created) header should be part of the signature string.
            if (signingSettings.SignatureAlgorithm.ShouldIncludeCreatedHeader() && !signingSettings.Headers.Contains(HeaderName.PredefinedHeaderNames.Created)) {
                signingSettings.Headers = AppendHeaderName(signingSettings.Headers, HeaderName.PredefinedHeaderNames.Created);
            }

            // According to the spec, when the algorithm does not start with 'rsa', 'hmac' or 'ecdsa', the (expires) header should be part of the signature string.
            if (signingSettings.SignatureAlgorithm.ShouldIncludeExpiresHeader() && !signingSettings.Headers.Contains(HeaderName.PredefinedHeaderNames.Expires)) {
                signingSettings.Headers = AppendHeaderName(signingSettings.Headers, HeaderName.PredefinedHeaderNames.Expires);
            }

            // When digest is enabled, make it part of the signature string
            if (!string.IsNullOrEmpty(signingSettings.DigestHashAlgorithm.Name) && request.Method.SupportsBody() &&
                !signingSettings.Headers.Contains(HeaderName.PredefinedHeaderNames.Digest)) {
                signingSettings.Headers = AppendHeaderName(signingSettings.Headers, HeaderName.PredefinedHeaderNames.Digest);
            }
        }

        private static HeaderName[] AppendHeaderName(HeaderName[] headerNames, HeaderName toAppend) {
            var result = new HeaderName[headerNames.Length + 1];
            headerNames.CopyTo(result, 0);
            result[headerNames.Length] = toAppend;
            return result;
        }
    }
}