namespace Dalion.HttpMessageSigning.Signing {
    internal interface INonceGenerator {
        string GenerateNonce();
    }
}