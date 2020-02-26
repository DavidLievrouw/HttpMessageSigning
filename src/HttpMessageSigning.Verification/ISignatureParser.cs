namespace Dalion.HttpMessageSigning.Verification {
    internal interface ISignatureParser {
        Signature Parse(HttpRequestForSigning request);
    }
}