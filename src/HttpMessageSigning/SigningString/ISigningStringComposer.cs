using System;

namespace Dalion.HttpMessageSigning.SigningString {
    internal interface ISigningStringComposer {
        string Compose(HttpRequestForSigning request, string signatureAlgorithmName, HeaderName[] headerNames, DateTimeOffset timeOfComposing, TimeSpan expires, string nonce);
    }
}