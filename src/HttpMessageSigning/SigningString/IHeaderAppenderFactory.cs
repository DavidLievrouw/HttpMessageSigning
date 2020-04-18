using System;

namespace Dalion.HttpMessageSigning.SigningString {
    internal interface IHeaderAppenderFactory {
        IHeaderAppender Create(HttpRequestForSigning request, string signatureAlgorithmName, DateTimeOffset timeOfComposing, TimeSpan expires);
    }
}