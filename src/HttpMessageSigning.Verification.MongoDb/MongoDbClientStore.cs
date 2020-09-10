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
                NonceExpiration = client.NonceLifetime.TotalSeconds,
                ClockSkew = client.ClockSkew.TotalSeconds,
                Claims = client.Claims?.Select(ClaimDataRecord.FromClaim)?.ToArray(),
                SignatureAlgorithm = SignatureAlgorithmDataRecord.FromSignatureAlgorithm(client.SignatureAlgorithm),
                RequestTargetEscaping = client.RequestTargetEscaping.ToString()
            };

            var collection = _lazyCollection.Value;

            return collection.ReplaceOneAsync(r => r.Id == record.Id, record, new ReplaceOptions {IsUpsert = true});
        }

        public async Task<Client> Get(KeyId clientId) {
            if (clientId == KeyId.Empty) throw new ArgumentException("Value cannot be null or empty.", nameof(clientId));

            var collection = _lazyCollection.Value;

            var findResult = await collection.FindAsync(r => r.Id == clientId).ConfigureAwait(continueOnCapturedContext: false);
            var matches = await findResult.ToListAsync().ConfigureAwait(continueOnCapturedContext: false);
            if (!matches.Any()) return null;

            var match = matches.Single();

            var nonceExpiration = !match.NonceExpiration.HasValue || match.NonceExpiration.Value <= 0.0
                ? ClientOptions.DefaultNonceLifetime
                : TimeSpan.FromSeconds(match.NonceExpiration.Value);
            
            var clockSkew = !match.ClockSkew.HasValue || match.ClockSkew.Value <= 0.0
                ? ClientOptions.DefaultClockSkew
                : TimeSpan.FromSeconds(match.ClockSkew.Value);

            var requestTargetEscaping = RequestTargetEscaping.RFC3986;
            if (!string.IsNullOrEmpty(match.RequestTargetEscaping)) {
                if (Enum.TryParse<RequestTargetEscaping>(match.RequestTargetEscaping, ignoreCase: true, out var parsed)) {
                    requestTargetEscaping = parsed;
                }
            }
            
            return new Client(
                match.Id,
                match.Name,
                match.SignatureAlgorithm.ToSignatureAlgorithm(),
                nonceExpiration,
                clockSkew,
                requestTargetEscaping,
                match.Claims?.Select(c => c.ToClaim())?.ToArray());
        }
    }
}