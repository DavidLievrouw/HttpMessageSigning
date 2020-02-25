namespace Dalion.HttpMessageSigning {
    public abstract class Secret {
        public abstract SignatureAlgorithm Algorithm { get; }
    }
}