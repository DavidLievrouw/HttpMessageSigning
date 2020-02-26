using System;

namespace Dalion.HttpMessageSigning.SigningString {
    internal interface IHeaderAppenderFactory {
        IHeaderAppender Create(HttpRequestForSigning request, SigningSettings settings, DateTimeOffset timeOfComposing);
    }
}