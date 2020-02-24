namespace Dalion.HttpMessageSigning.Signing {
    public interface IRequestSignerFactory {
        IRequestSigner Create(SigningSettings signingSettings);
    }
}