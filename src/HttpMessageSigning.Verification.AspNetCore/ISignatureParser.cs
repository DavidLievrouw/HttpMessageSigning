using Microsoft.AspNetCore.Http;

namespace Dalion.HttpMessageSigning.Verification.AspNetCore {
    internal interface ISignatureParser {
        SignatureParsingResult Parse(HttpRequest request, SignedRequestAuthenticationOptions options);
    }
}