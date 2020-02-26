using System.Collections.Generic;

namespace Dalion.HttpMessageSigning.Verification {
    internal class DefaultSignatureHeadersProvider : IDefaultSignatureHeadersProvider {
        public HeaderName[] ProvideDefaultHeaders(ISignatureAlgorithm signatureAlgorithm) {
            var list = new List<HeaderName> {HeaderName.PredefinedHeaderNames.RequestTarget};
            if (signatureAlgorithm.ShouldIncludeDateHeader()) list.Add(HeaderName.PredefinedHeaderNames.Date);
            if (signatureAlgorithm.ShouldIncludeCreatedHeader()) list.Add(HeaderName.PredefinedHeaderNames.Created);
            if (signatureAlgorithm.ShouldIncludeExpiresHeader()) list.Add(HeaderName.PredefinedHeaderNames.Expires);
            return list.ToArray();
        }
    }
}