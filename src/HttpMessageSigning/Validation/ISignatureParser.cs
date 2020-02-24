using Microsoft.AspNetCore.Http;

namespace Dalion.HttpMessageSigning.Validation {
    public interface ISignatureParser {
        Signature Parse(HttpRequest request);
    }
}