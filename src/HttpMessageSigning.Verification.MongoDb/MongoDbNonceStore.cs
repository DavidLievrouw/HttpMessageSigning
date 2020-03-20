using System;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    internal class MongoDbNonceStore : INonceStore {
        private readonly Lazy<IMongoCollection<ClientDataRecord>> _lazyCollection;

        public MongoDbNonceStore(IMongoDatabaseClientProvider clientProvider, string collectionName) {
            if (clientProvider == null) throw new ArgumentNullException(nameof(clientProvider));
            if (string.IsNullOrEmpty(collectionName)) throw new ArgumentException("Value cannot be null or empty.", nameof(collectionName));
            
            _lazyCollection = new Lazy<IMongoCollection<ClientDataRecord>>(() => {
                var database = clientProvider.Provide();
                return database.GetCollection<ClientDataRecord>(collectionName);
            });
        }
        
        public void Dispose() {
            // Noop
        }

        public Task Register(Nonce nonce) {
            throw new System.NotImplementedException();
        }

        public Task<Nonce> Get(KeyId clientId, string nonceValue) {
            throw new System.NotImplementedException();
        }
    }
}