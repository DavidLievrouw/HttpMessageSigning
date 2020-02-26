namespace Dalion.HttpMessageSigning.Verification {
    internal interface IDefaultSignatureHeadersProvider {
        HeaderName[] ProvideDefaultHeaders(ISignatureAlgorithm signatureAlgorithm);
    }
}