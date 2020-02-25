namespace Dalion.HttpMessageSigning {
    internal interface ISignatureAlgorithmFactory {
        ISignatureAlgorithm Create(SignatureAlgorithm signatureAlgorithm, HashAlgorithm hashAlgorithm, string secret);
    }
}