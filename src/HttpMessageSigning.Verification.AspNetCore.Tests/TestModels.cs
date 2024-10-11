using System;

namespace Dalion.HttpMessageSigning.Verification.AspNetCore {
    internal static class TestModels {
        public static readonly Signature Signature = new Signature {
            KeyId = (KeyId)"app1",
            Algorithm = "hs2019",
            Headers = new[] {
                HeaderName.PredefinedHeaderNames.RequestTarget,
                HeaderName.PredefinedHeaderNames.Date,
                (HeaderName) "dalion-app-id"
            },
            Created = new DateTimeOffset(2020, 2, 27, 14, 18, 22, TimeSpan.Zero),
            Expires = new DateTimeOffset(2020, 2, 27, 14, 21, 22, TimeSpan.Zero),
            String = "xyz123=",
            Nonce = "abc123"
        };
    }
}