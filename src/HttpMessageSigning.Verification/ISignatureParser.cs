using Microsoft.AspNetCore.Http;

namespace Dalion.HttpMessageSigning.Validation {
    internal interface ISignatureParser {
        Signature Parse(HttpRequest request);
    }
}