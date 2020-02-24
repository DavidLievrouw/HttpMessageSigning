namespace Dalion.HttpMessageSigning {
    internal interface IKeyedHashAlgorithm {
        byte[] ComputeHash(string input);
    }
}