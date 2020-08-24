using System;
using MongoDB.Driver;

namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    internal class MongoDatabaseClientProvider : IMongoDatabaseClientProvider {
        private readonly IMongoDatabase _mongoDatabase;
        private readonly string _connectionString;
        
        public MongoDatabaseClientProvider(string connectionString) {
            if (string.IsNullOrEmpty(connectionString)) throw new ArgumentException("Value cannot be null or empty.", nameof(connectionString));
            _connectionString = connectionString;
        }

        public MongoDatabaseClientProvider(IMongoDatabase mongoDatabase) {
            _mongoDatabase = mongoDatabase ?? throw new ArgumentNullException(nameof(mongoDatabase));
        }

        public IMongoDatabase Provide() {
            if (_mongoDatabase != null) return _mongoDatabase;
            
            var mongoUrl = new MongoUrl(_connectionString);
            var clientSettings = MongoClientSettings.FromUrl(mongoUrl);
            var client = new MongoClient(clientSettings);

            return client.GetDatabase(mongoUrl.DatabaseName);
        }
    }
}