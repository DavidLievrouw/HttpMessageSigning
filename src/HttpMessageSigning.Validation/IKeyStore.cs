namespace Dalion.HttpMessageSigning.Validation {
    public interface IKeyStore {
        void Register(KeyStoreEntry entry);
        KeyStoreEntry Get(string id);
    }
}