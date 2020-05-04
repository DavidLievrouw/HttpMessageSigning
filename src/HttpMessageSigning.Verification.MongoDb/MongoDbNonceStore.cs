using System;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    internal class MongoDbNonceStore : IMongoDbNonceStore {
        private readonly Lazy<IMongoCollection<NonceDataRecord>> _lazyCollection;

        public MongoDbNonceStore(IMongoDatabaseClientProvider clientProvider, string collectionName) {
            if (clientProvider == null) throw new ArgumentNullException(nameof(clientProvider));
            if (string.IsNullOrEmpty(collectionName)) throw new ArgumentException("Value cannot be null or empty.", nameof(collectionName));

            _lazyCollection = new Lazy<IMongoCollection<NonceDataRecord>>(() => {
                var database = clientProvider.Provide();
                var collection = database.GetCollection<NonceDataRecord>(collectionName);

                var createIndexModel = new CreateIndexModel<NonceDataRecord>(
                    Builders<NonceDataRecord>.IndexKeys.Ascending(_ => _.Expiration),
                    new CreateIndexOptions {
                        Name = "idx_ttl",
                        ExpireAfter = TimeSpan.FromSeconds(3)
                    });
                try {
                    collection.Indexes.CreateOne(createIndexModel);
                }
                catch (MongoCommandException ex) {
                    switch (ex.Code) {
                        case 86:
                            // The index probably exists with different options, recreate it instead
                            collection.Indexes.DropOne("idx_ttl");
                            collection.Indexes.CreateOne(createIndexModel);
                            break;
                        default:
                            throw;
                    }
                }
                
                return collection;
            });
        }

        public void Dispose() {
            // Noop
        }

        public async Task Register(Nonce nonce) {
            if (nonce == null) throw new ArgumentNullException(nameof(nonce));

            var nonceId = CreateId(nonce.ClientId, nonce.Value);
            var record = new NonceDataRecord {
                Id = nonceId,
                ClientId = nonce.ClientId,
                Value = nonce.Value,
                Expiration = nonce.Expiration.UtcDateTime
            };

            var collection = _lazyCollection.Value;
            
            await collection.ReplaceOneAsync(r => r.Id == record.Id, record, new ReplaceOptions {IsUpsert = true}).ConfigureAwait(false);
        }

        public async Task<Nonce> Get(KeyId clientId, string nonceValue) {
            if (clientId == KeyId.Empty) throw new ArgumentException("Value cannot be null or empty.", nameof(clientId));
            if (string.IsNullOrEmpty(nonceValue)) throw new ArgumentException("Value cannot be null or empty.", nameof(nonceValue));

            var collection = _lazyCollection.Value;
            
            var nonceId = CreateId(clientId, nonceValue);
            var findResult = await collection.FindAsync(r => r.Id == nonceId).ConfigureAwait(false);
            var matches = await findResult.ToListAsync().ConfigureAwait(false);
            if (!matches.Any()) return null;

            var match = matches.OrderByDescending(_ => _.Expiration).First();

            return new Nonce(new KeyId(match.ClientId), match.Value, match.Expiration);
        }

        private static string CreateId(KeyId clientId, string nonceValue) {
            return $"{clientId}_{nonceValue}";
        }
    }
}