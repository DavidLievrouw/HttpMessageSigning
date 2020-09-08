namespace Dalion.HttpMessageSigning.SigningString {
    internal interface ISigningStringComposer {
        string Compose(SigningStringCompositionRequest compositionRequest);
    }
}