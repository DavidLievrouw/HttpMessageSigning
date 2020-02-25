namespace Dalion.HttpMessageSigning {
    internal interface ISignatureAlgorithmFactory {
        ISignatureAlgorithm Create(Secret secret, HashAlgorithm hashAlgorithm);
    }
}