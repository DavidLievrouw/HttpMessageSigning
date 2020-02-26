using System;

namespace Dalion.HttpMessageSigning.SigningString {
    internal interface ISigningStringComposer {
        string Compose(HttpRequestForSigning request, SigningSettings settings, DateTimeOffset timeOfComposing);
    }
}