using System;

namespace Dalion.HttpMessageSigning.SigningString {
    internal interface ISigningStringComposer {
        string Compose(HttpRequestForSigning request, HeaderName[] headerNames, DateTimeOffset timeOfComposing, TimeSpan expires);
    }
}