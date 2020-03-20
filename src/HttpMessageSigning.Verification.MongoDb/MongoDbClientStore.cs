using System;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    internal class MongoDbClientStore : IMongoDbClientStore {
        private readonly Lazy<IMongoCollection<ClientDataRecord>> _lazyCollection;

        public MongoDbClientStore(IMongoDatabaseClientProvider clientProvider, string collectionName) {
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

        public Task Register(Client client) {
            if (client == null) throw new ArgumentNullException(nameof(client));

            var record = new ClientDataRecord {
                Id = client.Id,
                Name = client.Name,
                Claims = client.Claims?.Select(c => ClaimDataRecord.FromClaim(c))?.ToArray(),
                SignatureAlgorithm = SignatureAlgorithmDataRecord.FromSignatureAlgorithm(client.SignatureAlgorithm)
            };

            var collection = _lazyCollection.Value;

            return collection.ReplaceOneAsync(r => r.Id == record.Id, record, new ReplaceOptions {IsUpsert = true});
        }

        public async Task<Client> Get(KeyId id) {
            if (id == KeyId.Empty) throw new ArgumentException("Value cannot be null or empty.", nameof(id));

            var collection = _lazyCollection.Value;

            var findResult = await collection.FindAsync(r => r.Id == id);
            var matches = await findResult.ToListAsync();
            if (!matches.Any()) throw new SignatureVerificationException($"No {nameof(Client)}s with id '{id}' are registered in the server store.");

            var match = matches.Single();

            return new Client(
                match.Id,
                match.Name,
                match.SignatureAlgorithm.ToSignatureAlgorithm(),
                match.Claims?.Select(c => c.ToClaim())?.ToArray());
        }
    }
}