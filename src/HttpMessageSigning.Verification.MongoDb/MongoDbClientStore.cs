using System;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    internal class MongoDbClientStore : IClientStore {
        private readonly MongoDbSettings _mongoDbSettings;
        
        public MongoDbClientStore(MongoDbSettings mongoDbSettings) {
            _mongoDbSettings = mongoDbSettings ?? throw new ArgumentNullException(nameof(mongoDbSettings));
        }

        public void Dispose() {
            throw new NotImplementedException();
        }

        public Task Register(Client client) {
            throw new NotImplementedException();
        }

        public Task<Client> Get(string id) {
            throw new NotImplementedException();
        }
    }
}