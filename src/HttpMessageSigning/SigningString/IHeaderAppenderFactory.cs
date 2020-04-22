using System;

namespace Dalion.HttpMessageSigning.SigningString {
    internal interface IHeaderAppenderFactory {
        IHeaderAppender Create(HttpRequestForSigning request, DateTimeOffset timeOfComposing, TimeSpan expires);
    }
}