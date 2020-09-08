using System;
using System.Collections.Generic;
using System.Net.Http;
using Dalion.HttpMessageSigning.TestUtils;
using Microsoft.Extensions.Primitives;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    internal static class TestModels {
        public static readonly Signature Signature = new Signature {
            KeyId = "app1",
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
        
        public static readonly HttpRequestForVerification RequestForVerification = new HttpRequestForVerification {
            RequestUri = "https://dalion.eu/api/rsc1".ToUri(),
            Method = HttpMethod.Get,
            Headers = new HeaderDictionary(new Dictionary<string, StringValues> {
                {"dalion-app-id", "app-one"},
                {HeaderName.PredefinedHeaderNames.Date, Signature.Created.Value.ToString("R")}
            }),
            Signature = new Signature { KeyId = new KeyId("app-one")}
        };

        public static readonly Client Client = new Client(Signature.KeyId, "Unit test app", new CustomSignatureAlgorithm("HMAC"), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1.5));
    }
}