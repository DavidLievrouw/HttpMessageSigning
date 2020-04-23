using Microsoft.Owin;

namespace Dalion.HttpMessageSigning.Verification.Owin {
    internal interface ISignatureParser {
        Signature Parse(IOwinRequest request, SignedHttpRequestAuthenticationOptions options);
    }
}