namespace Dalion.HttpMessageSigning.Signing {
    internal class NonceGenerator : INonceGenerator {
        public string GenerateNonce() {
            return ShortGuid.NewGuid().Value;
        }
    }
}