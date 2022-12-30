using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
            }, LazyThreadSafetyMode.PublicationOnly);
        }

        public async Task Run() {
            var collection = _lazyCollection.Value;
            if (collection == null) {
                throw new InvalidOperationException("Could not find the collection to migrate. Please check your MongoDB connection string.");
            }

            var allClientsQueryTask = collection.FindAsync(FilterDefinition<ClientDataRecordV2>.Empty);
            var allClients = allClientsQueryTask != null 
                ? (await allClientsQueryTask.ConfigureAwait(continueOnCapturedContext: false)).ToList() 
                : new List<ClientDataRecordV2>();
            
            var clientsToMigrate = allClients
                .Where(c => !c.V.HasValue || c.V < 2)
                .ToList();

            foreach (var clientToMigrate in clientsToMigrate) {
                // Encrypt parameter, if needed:
                // - Encryption should be enabled
                // - It should not have been encrypted before
                // - Only applicable for HMAC signature algorithms (the only supported symmetric key algorithm)
                if (_mongoDbClientStoreSettings.SharedSecretEncryptionKey != SharedSecretEncryptionKey.Empty &&
                    StringComparer.OrdinalIgnoreCase.Equals("hmac", clientToMigrate.SignatureAlgorithm.Type) &&
                    !(clientToMigrate.SignatureAlgorithm?.IsParameterEncrypted ?? false)) {
                    var protector = _stringProtectorFactory.CreateSymmetric(_mongoDbClientStoreSettings.SharedSecretEncryptionKey);
                    if (clientToMigrate.SignatureAlgorithm != null) {
                        clientToMigrate.SignatureAlgorithm.Parameter = protector.Protect(clientToMigrate.SignatureAlgorithm.Parameter);
                        clientToMigrate.SignatureAlgorithm.IsParameterEncrypted = true;
                    }
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