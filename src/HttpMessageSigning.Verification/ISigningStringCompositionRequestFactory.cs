using Dalion.HttpMessageSigning.SigningString;

namespace Dalion.HttpMessageSigning.Verification {
    internal interface ISigningStringCompositionRequestFactory {
        SigningStringCompositionRequest CreateForVerification(HttpRequestForSigning request, Client client, Signature signature);
    }
}