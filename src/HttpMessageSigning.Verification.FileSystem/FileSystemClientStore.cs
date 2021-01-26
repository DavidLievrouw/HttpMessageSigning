using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification.FileSystem {
    internal class FileSystemClientStore : IClientStore {
        public void Dispose() {
            throw new System.NotImplementedException();
        }

        public Task Register(Client client) {
            throw new System.NotImplementedException();
        }

        public Task<Client> Get(KeyId clientId) {
            throw new System.NotImplementedException();
        }
    }
}