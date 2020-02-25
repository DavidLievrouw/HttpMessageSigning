using Microsoft.AspNetCore.Http;

namespace Dalion.HttpMessageSigning.Verification {
    internal interface ISignatureParser {
        Signature Parse(HttpRequest request);
    }
}