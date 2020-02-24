using System.Net.Http;

namespace Dalion.HttpMessageSigning.Signing {
    internal interface ISignatureCreator {
        Signature CreateSignature(HttpRequestMessage request, SigningSettings settings);
    }
}