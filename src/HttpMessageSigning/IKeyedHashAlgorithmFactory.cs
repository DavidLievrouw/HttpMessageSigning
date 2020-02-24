namespace Dalion.HttpMessageSigning {
    internal interface IKeyedHashAlgorithmFactory {
        IKeyedHashAlgorithm Create(SignatureAlgorithm signatureAlgorithm, HashAlgorithm hashAlgorithm, string secret);
    }
}