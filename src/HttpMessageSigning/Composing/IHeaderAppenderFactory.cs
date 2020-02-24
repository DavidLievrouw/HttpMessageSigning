using System.Net.Http;

namespace Dalion.HttpMessageSigning.Composing {
    internal interface IHeaderAppenderFactory {
        IHeaderAppender Create(HttpRequestMessage request, SigningSettings settings);
    }
}