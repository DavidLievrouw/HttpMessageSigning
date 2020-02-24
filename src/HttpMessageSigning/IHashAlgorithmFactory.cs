namespace Dalion.HttpMessageSigning {
    internal interface IHashAlgorithmFactory {
        IHashAlgorithm Create(HashAlgorithm hashAlgorithm);
    }
}