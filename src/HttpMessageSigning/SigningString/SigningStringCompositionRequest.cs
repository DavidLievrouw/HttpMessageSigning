using System;

namespace Dalion.HttpMessageSigning.SigningString {
    internal class SigningStringCompositionRequest {
        public HttpRequestForSignatureString Request { get; set; }
        public RequestTargetEscaping RequestTargetEscaping { get; set; }
        public HeaderName[] HeadersToInclude { get; set; }
        public DateTimeOffset? TimeOfComposing { get; set; }
        public TimeSpan? Expires { get; set; }
        public string Nonce { get; set; }
    }
}