using System;
using Dalion.HttpMessageSigning.SigningString;

namespace Dalion.HttpMessageSigning.Signing {
    internal interface ISigningStringCompositionRequestFactory {
        SigningStringCompositionRequest CreateForSigning(HttpRequestForSigning request, SigningSettings signingSettings, DateTimeOffset? timeOfComposing, TimeSpan? expires);
    }
}