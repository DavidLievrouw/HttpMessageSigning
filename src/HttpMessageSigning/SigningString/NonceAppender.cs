namespace Dalion.HttpMessageSigning.SigningString {
    internal class NonceAppender : INonceAppender {
        public string BuildStringToAppend(string nonce) {
            return string.IsNullOrEmpty(nonce)
                ? string.Empty
                : $"\nnonce: {nonce}";
        }
    }
}