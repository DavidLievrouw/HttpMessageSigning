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
                signingSettings.Headers = new[] {HeaderName.PredefinedHeaderNames.RequestTarget}.Concat(signingSettings.Headers).ToArray();
            }

            // According to the spec, when the algorithm starts with 'rsa', 'hmac' or 'ecdsa', the Date header should be part of the signature string.
            if (signingSettings.SignatureAlgorithm.ShouldIncludeDateHeader() && !signingSettings.Headers.Contains(HeaderName.PredefinedHeaderNames.Date)) {
                signingSettings.Headers = signingSettings.Headers.Concat(new[] {HeaderName.PredefinedHeaderNames.Date}).ToArray();
            }

            // According to the spec, when the algorithm does not start with 'rsa', 'hmac' or 'ecdsa', the (created) header should be part of the signature string.
            if (signingSettings.SignatureAlgorithm.ShouldIncludeCreatedHeader() && !signingSettings.Headers.Contains(HeaderName.PredefinedHeaderNames.Created)) {
                signingSettings.Headers = signingSettings.Headers.Concat(new[] {HeaderName.PredefinedHeaderNames.Created}).ToArray();
            }

            // According to the spec, when the algorithm does not start with 'rsa', 'hmac' or 'ecdsa', the (expires) header should be part of the signature string.
            if (signingSettings.SignatureAlgorithm.ShouldIncludeExpiresHeader() && !signingSettings.Headers.Contains(HeaderName.PredefinedHeaderNames.Expires)) {
                signingSettings.Headers = signingSettings.Headers.Concat(new[] {HeaderName.PredefinedHeaderNames.Expires}).ToArray();
            }

            // When digest is enabled, make it part of the signature string
            if (!string.IsNullOrEmpty(signingSettings.DigestHashAlgorithm.Name) && request.Method.SupportsBody() &&
                !signingSettings.Headers.Contains(HeaderName.PredefinedHeaderNames.Digest)) {
                signingSettings.Headers = signingSettings.Headers.Concat(new[] {HeaderName.PredefinedHeaderNames.Digest}).ToArray();
            }
        }
    }
}