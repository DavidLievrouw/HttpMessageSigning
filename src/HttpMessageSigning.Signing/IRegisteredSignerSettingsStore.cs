namespace Dalion.HttpMessageSigning.Signing {
    internal interface IRegisteredSignerSettingsStore {
        SigningSettings Get(KeyId keyId);
    }
}