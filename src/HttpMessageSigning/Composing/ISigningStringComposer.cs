using System.Net.Http;

namespace Dalion.HttpMessageSigning.Composing {
    internal interface ISigningStringComposer {
        string Compose(HttpRequestMessage request, SigningSettings settings);
    }
}