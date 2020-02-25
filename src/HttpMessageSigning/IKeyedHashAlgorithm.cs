namespace Dalion.HttpMessageSigning {
    internal interface IKeyedHashAlgorithm : IHashAlgorithm {
        byte[] Key { get; }
    }
}