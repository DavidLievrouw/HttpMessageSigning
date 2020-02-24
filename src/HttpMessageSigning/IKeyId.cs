namespace Dalion.HttpMessageSigning {
    public interface IKeyId {
        HashAlgorithm HashAlgorithm { get; }
        SignatureAlgorithm SignatureAlgorithm { get; }
        string Key { get; }
    }
}