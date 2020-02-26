using System.Collections.Generic;

namespace Dalion.HttpMessageSigning.Verification {
    internal class DefaultSignatureHeadersProvider : IDefaultSignatureHeadersProvider {
        public HeaderName[] ProvideDefaultHeaders(ISignatureAlgorithm signatureAlgorithm) {
            var list = new List<HeaderName> {HeaderName.PredefinedHeaderNames.RequestTarget};
            if (ShouldHaveCreatedHeader(signatureAlgorithm)) list.Add(HeaderName.PredefinedHeaderNames.Created);
            if (ShouldHaveExpiresHeader(signatureAlgorithm)) list.Add(HeaderName.PredefinedHeaderNames.Expires);
            if (ShouldHaveDateHeader(signatureAlgorithm)) list.Add(HeaderName.PredefinedHeaderNames.Date);
            return list.ToArray();
        }

        private static bool ShouldHaveDateHeader(ISignatureAlgorithm signatureAlgorithm) {
            var signatureAlgorithmString = signatureAlgorithm.Name.ToLowerInvariant();
            return signatureAlgorithmString.StartsWith("rsa") ||
                   signatureAlgorithmString.StartsWith("hmac") ||
                   signatureAlgorithmString.StartsWith("ecdsa");
        }

        private static bool ShouldHaveCreatedHeader(ISignatureAlgorithm signatureAlgorithm) {
            var signatureAlgorithmString = signatureAlgorithm.Name.ToLowerInvariant();
            return !signatureAlgorithmString.StartsWith("rsa") &&
                   !signatureAlgorithmString.StartsWith("hmac") &&
                   !signatureAlgorithmString.StartsWith("ecdsa");
        }

        private static bool ShouldHaveExpiresHeader(ISignatureAlgorithm signatureAlgorithm) {
            var signatureAlgorithmString = signatureAlgorithm.Name.ToLowerInvariant();
            return !signatureAlgorithmString.StartsWith("rsa") &&
                   !signatureAlgorithmString.StartsWith("hmac") &&
                   !signatureAlgorithmString.StartsWith("ecdsa");
        }
    }
}