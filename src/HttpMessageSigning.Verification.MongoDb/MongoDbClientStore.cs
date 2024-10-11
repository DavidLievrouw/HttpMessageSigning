using System;
using System.Linq;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.Verification.MongoDb.ClientStoreMigrations;
using MongoDB.Driver;

namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    internal class MongoDbClientStore : IClientStore {
        private readonly SharedSecretEncryptionKey _encryptionKey;
        private readonly IClientStoreMigrator _migrator;
        private readonly ISignatureAlgorithmDataRecordConverter _signatureAlgorithmDataRecordConverter;
        private readonly Lazy<IMongoCollection<ClientDataRecordV2>> _lazyCollection;

        public MongoDbClientStore(
            IMongoDatabaseClientProvider clientProvider, 
            string collectionName, 
            SharedSecretEncryptionKey encryptionKey,
            IClientStoreMigrator migrator,
            ISignatureAlgorithmDataRecordConverter signatureAlgorithmDataRecordConverter) {
            if (clientProvider == null) throw new ArgumentNullException(nameof(clientProvider));
            if (string.IsNullOrEmpty(collectionName)) throw new ArgumentException("Value cannot be null or empty.", nameof(collectionName));
            _encryptionKey = encryptionKey;
            _migrator = migrator ?? throw new ArgumentNullException(nameof(migrator));
            _signatureAlgorithmDataRecordConverter = signatureAlgorithmDataRecordConverter ?? throw new ArgumentNullException(nameof(signatureAlgorithmDataRecordConverter));

            _lazyCollection = new Lazy<IMongoCollection<ClientDataRecordV2>>(() => {
                var database = clientProvider.Provide();
                return database.GetCollection<ClientDataRecordV2>(collectionName);
            });
        }

        public void Dispose() {
            // Noop
        }

        public async Task Register(Client client) {
            if (client == null) throw new ArgumentNullException(nameof(client));

            await _migrator.Migrate().ConfigureAwait(continueOnCapturedContext: false);
            
            if (IsProhibitedId(client.Id)) throw new ArgumentException($"The id value of the specified {nameof(Client)} is prohibited ({client.Id}).", nameof(client));
            
            var record = new ClientDataRecordV2 {
                Id = client.Id,
                Name = client.Name,
                NonceLifetime = client.NonceLifetime.TotalSeconds,
                ClockSkew = client.ClockSkew.TotalSeconds,
                Claims = client.Claims?.Select(ClaimDataRecordV2.FromClaim)?.ToArray(),
                SignatureAlgorithm = _signatureAlgorithmDataRecordConverter.FromSignatureAlgorithm(client.SignatureAlgorithm, _encryptionKey),
                RequestTargetEscaping = client.RequestTargetEscaping.ToString()
            };
            record.V = ClientDataRecordV2.GetV();

            var collection = _lazyCollection.Value;

            await collection
                .ReplaceOneAsync(r => r.Id == record.Id, record, new ReplaceOptions {IsUpsert = true})
                .ConfigureAwait(continueOnCapturedContext: false);
        }

        public async Task<Client> Get(KeyId clientId) {
            if (clientId == KeyId.Empty) throw new ArgumentException("Value cannot be null or empty.", nameof(clientId));
            
            await _migrator.Migrate().ConfigureAwait(continueOnCapturedContext: false);

            if (IsProhibitedId(clientId)) return null;
            
            var collection = _lazyCollection.Value;

            var findResult = await collection.FindAsync(r => r.Id == clientId.Value).ConfigureAwait(continueOnCapturedContext: false);
            var matches = await findResult.ToListAsync().ConfigureAwait(continueOnCapturedContext: false);
            if (!matches.Any()) return null;

            var match = matches.Single();

            var nonceLifetime = !match.NonceLifetime.HasValue || match.NonceLifetime.Value <= 0.0
                ? ClientOptions.Default.NonceLifetime
                : TimeSpan.FromSeconds(match.NonceLifetime.Value);
            
            var clockSkew = !match.ClockSkew.HasValue || match.ClockSkew.Value <= 0.0
                ? ClientOptions.Default.ClockSkew
                : TimeSpan.FromSeconds(match.ClockSkew.Value);

            var requestTargetEscaping = RequestTargetEscaping.RFC3986;
            if (!string.IsNullOrEmpty(match.RequestTargetEscaping)) {
                if (Enum.TryParse<RequestTargetEscaping>(match.RequestTargetEscaping, ignoreCase: true, out var parsed)) {
                    requestTargetEscaping = parsed;
                }
            }

            var signatureAlgorithm = _signatureAlgorithmDataRecordConverter.ToSignatureAlgorithm(match.SignatureAlgorithm, _encryptionKey, match.V);
            
            return new Client(
                (KeyId)match.Id,
                match.Name,
                signatureAlgorithm,
                nonceLifetime,
                clockSkew,
                requestTargetEscaping,
                match.Claims?.Select(c => c.ToClaim())?.ToArray());
        }
        
        private static bool IsProhibitedId(KeyId id) {
            return ProhibitedIds.Contains(id);
        }

        private static readonly KeyId[] ProhibitedIds = {
            KeyId.Empty,
            (KeyId)"_version"
        };
    }
}