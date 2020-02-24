using System;
using System.Net.Http;

namespace Dalion.HttpMessageSigning.SigningString {
    internal interface IHeaderAppenderFactory {
        IHeaderAppender Create(HttpRequestMessage request, SigningSettings settings, DateTimeOffset timeOfComposing);
    }
}