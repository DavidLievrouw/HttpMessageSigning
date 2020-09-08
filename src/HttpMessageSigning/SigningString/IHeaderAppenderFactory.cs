using System;

namespace Dalion.HttpMessageSigning.SigningString {
    internal interface IHeaderAppenderFactory {
        IHeaderAppender Create(HttpRequestForSigning request, RequestTargetEscaping requestTargetEscaping, DateTimeOffset? timeOfComposing, TimeSpan? expires);
    }
}