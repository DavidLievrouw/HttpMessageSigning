using Microsoft.AspNetCore.Http;

namespace Dalion.HttpMessageSigning.Verification.AspNetCore {
    internal interface ISignatureParser {
        Signature Parse(HttpRequest request);
    }
}