using System;
using System.Linq;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.Utils;
using MongoDB.Driver;

namespace Dalion.HttpMessageSigning.Verification.MongoDb.ClientStoreMigrations.V0002 {
    internal class AddEncryptionSupportToClientsMigrationStep : IClientStoreMigrationStep {
        private readonly MongoDbClientStoreSettings _mongoDbClientStoreSettings;
        private readonly IStringProtectorFactory _stringProtectorFactory;
        private readonly Lazy<IMongoCollection<ClientDataRecordV2>> _lazyCollection;

        public AddEncryptionSupportToClientsMigrationStep(
            IMongoDatabaseClientProvider clientProvider, 
            MongoDbClientStoreSettings mongoDbClientStoreSettings,
            IStringProtectorFactory stringProtectorFactory) {
            if (clientProvider == null) throw new ArgumentNullException(nameof(clientProvider));
            
            _mongoDbClientStoreSettings = mongoDbClientStoreSettings ?? throw new ArgumentNullException(nameof(mongoDbClientStoreSettings));
            _stringProtectorFactory = stringProtectorFactory ?? throw new ArgumentNullException(nameof(stringProtectorFactory));

            _lazyCollection = new Lazy<IMongoCollection<ClientDataRecordV2>>(() => {
                var database = clientProvider.Provide();
                return database.GetCollection<ClientDataRecordV2>(mongoDbClientStoreSettings.CollectionName);
            });
        }

        public async Task Run() {
            var collection = _lazyCollection.Value;
            
            var allClients = await collection.FindAsync(FilterDefinition<ClientDataRecordV2>.Empty).ConfigureAwait(continueOnCapturedContext: false);
            var clientsToMigrate = (await allClients.ToListAsync().ConfigureAwait(continueOnCapturedContext: false))
                .Where(c => !c.V.HasValue || c.V.Value < 2)
                .ToList();

            foreach (var clientToMigrate in clientsToMigrate) {
                // Encrypt parameter, if needed:
                // - Encryption should be enabled
                // - It should not have been encrypted before
                // - Only applicable for HMAC signature algorithms (the only supported symmetric key algorithm)
                if (_mongoDbClientStoreSettings.SharedSecretEncryptionKey != SharedSecretEncryptionKey.Empty &&
                    clientToMigrate.SignatureAlgorithm.Type.Equals("hmac", StringComparison.OrdinalIgnoreCase) &&
                    !clientToMigrate.SignatureAlgorithm.IsParameterEncrypted) {
                    var protector = _stringProtectorFactory.CreateSymmetric(_mongoDbClientStoreSettings.SharedSecretEncryptionKey);
                    clientToMigrate.SignatureAlgorithm.Parameter = protector.Protect(clientToMigrate.SignatureAlgorithm.Parameter);
                    clientToMigrate.SignatureAlgorithm.IsParameterEncrypted = true;
                } 
                
                // Fill in RequestTargetEscaping, if it is missing
                clientToMigrate.RequestTargetEscaping = ClientOptions.Default.RequestTargetEscaping.ToString();
                
                // Fill in ClockSkew, if it is missing
                clientToMigrate.ClockSkew = clientToMigrate.ClockSkew ?? ClientOptions.Default.ClockSkew.TotalSeconds;
                
                // Fill in NonceLifetime, if it is missing, and drop obsolete NonceExpiration value
#pragma warning disable 618
                clientToMigrate.NonceLifetime = clientToMigrate.NonceLifetime ?? clientToMigrate.NonceExpiration ?? ClientOptions.Default.NonceLifetime.TotalSeconds;
                clientToMigrate.NonceExpiration = null;
#pragma warning restore 618
                
                // Update version
                clientToMigrate.V = ClientDataRecordV2.GetV();

                // Store migrated client
                await collection
                    .ReplaceOneAsync(_ => _.Id == clientToMigrate.Id, clientToMigrate, new ReplaceOptions { IsUpsert = false })
                    .ConfigureAwait(continueOnCapturedContext: false);
            }
        }

        public int Version => 2;
    }
}