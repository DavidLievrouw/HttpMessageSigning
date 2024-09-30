using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Dalion.HttpMessageSigning.Signing {
    internal class SigningSettingsSanitizer : ISigningSettingsSanitizer {
        public void SanitizeHeaderNamesToInclude(SigningSettings signingSettings, HttpRequestMessage request) {
            if (signingSettings == null) throw new ArgumentNullException(nameof(signingSettings));
            if (request == null) throw new ArgumentNullException(nameof(request));

            // When feature is disabled, don't take any further action
            if (!signingSettings.AutomaticallyAddRecommendedHeaders) return;

            var headers = signingSettings.Headers != null 
                ? new HashSet<HeaderName>(signingSettings.Headers) 
                : new HashSet<HeaderName>();

            // According to the spec, the header (request-target) should always be a part of the signature string.
            headers.Add(HeaderName.PredefinedHeaderNames.RequestTarget);

            // According to the spec, when the algorithm starts with 'rsa', 'hmac' or 'ecdsa', the Date header should be part of the signature string.
            if (signingSettings.SignatureAlgorithm.ShouldIncludeDateHeader()) {
                headers.Add(HeaderName.PredefinedHeaderNames.Date);
            }

            // According to the spec, when the algorithm does not start with 'rsa', 'hmac' or 'ecdsa', the (created) header should be part of the signature string.
            if (signingSettings.SignatureAlgorithm.ShouldIncludeCreatedHeader()) {
                headers.Add(HeaderName.PredefinedHeaderNames.Created);
            }

            // According to the spec, when the algorithm does not start with 'rsa', 'hmac' or 'ecdsa', the (expires) header should be part of the signature string.
            if (signingSettings.SignatureAlgorithm.ShouldIncludeExpiresHeader()) {
                headers.Add(HeaderName.PredefinedHeaderNames.Expires);
            }

            // When digest is enabled, make it part of the signature string
            if (!string.IsNullOrEmpty(signingSettings.DigestHashAlgorithm.Name) && request.Method.SupportsBody()) {
                headers.Add(HeaderName.PredefinedHeaderNames.Digest);
            }

            //signingSettings.Headers = new List<HeaderName>(headers).ToArray();
            signingSettings.Headers = headers.ToArray();
        }
    }
}