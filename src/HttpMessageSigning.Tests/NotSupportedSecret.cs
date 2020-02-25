namespace Dalion.HttpMessageSigning {
    public class NotSupportedSecret : Secret {
        public override SignatureAlgorithm Algorithm => (SignatureAlgorithm) (999);
    }
}