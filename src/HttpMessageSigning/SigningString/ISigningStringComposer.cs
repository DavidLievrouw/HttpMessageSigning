using System;
using System.Net.Http;

namespace Dalion.HttpMessageSigning.SigningString {
    internal interface ISigningStringComposer {
        string Compose(HttpRequestMessage request, SigningSettings settings, DateTimeOffset timeOfComposing);
    }
}