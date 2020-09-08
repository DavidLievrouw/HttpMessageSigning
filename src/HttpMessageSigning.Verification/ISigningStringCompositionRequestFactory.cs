using Dalion.HttpMessageSigning.SigningString;

namespace Dalion.HttpMessageSigning.Verification {
    internal interface ISigningStringCompositionRequestFactory {
        SigningStringCompositionRequest CreateForVerification(HttpRequestForVerification request, Client client, Signature signature);
    }
}