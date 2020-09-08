using System;

namespace Dalion.HttpMessageSigning.SigningString {
    internal interface IHeaderAppenderFactory {
        IHeaderAppender Create(HttpRequestForSignatureString request, RequestTargetEscaping requestTargetEscaping, DateTimeOffset? timeOfComposing, TimeSpan? expires);
    }
}