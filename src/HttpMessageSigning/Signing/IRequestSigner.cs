using System.Net.Http;

namespace Dalion.HttpMessageSigning.Signing {
    internal interface IRequestSigner {
        void Sign(HttpRequestMessage request, SigningSettings signingSettings);
    }
}