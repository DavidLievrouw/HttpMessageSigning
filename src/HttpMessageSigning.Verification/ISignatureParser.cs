using System.Net.Http;

namespace Dalion.HttpMessageSigning.Verification {
    internal interface ISignatureParser {
        Signature Parse(HttpRequestMessage request);
    }
}