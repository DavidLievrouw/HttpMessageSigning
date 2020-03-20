namespace Dalion.HttpMessageSigning.SigningString {
    internal interface INonceAppender {
        string BuildStringToAppend(string nonce);
    }
}