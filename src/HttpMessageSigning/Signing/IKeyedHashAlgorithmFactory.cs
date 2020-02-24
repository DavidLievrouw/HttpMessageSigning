namespace Dalion.HttpMessageSigning.Signing {
    internal interface IKeyedHashAlgorithmFactory {
        IKeyedHashAlgorithm Create(SignatureAlgorithm signatureAlgorithm, HashAlgorithm hashAlgorithm, byte[] signingKey);
    }
}