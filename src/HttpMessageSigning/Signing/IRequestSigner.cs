using System.Net.Http;

namespace Dalion.HttpMessageSigning.Signing {
    public interface IRequestSigner {
        void Sign(HttpRequestMessage request);
    }
}