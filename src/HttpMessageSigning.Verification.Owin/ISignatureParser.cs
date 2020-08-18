using Microsoft.Owin;

namespace Dalion.HttpMessageSigning.Verification.Owin {
    internal interface ISignatureParser {
        SignatureParsingResult Parse(IOwinRequest request, SignedHttpRequestAuthenticationOptions options);
    }
}