using System;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.Utils;
using MongoDB.Driver;

namespace Dalion.HttpMessageSigning.Verification.MongoDb.ClientStoreMigrations {
    internal class ClientStoreBaseliner : IClientStoreBaseliner {
        private readonly ISystemClock _systemClock;
        private readonly Lazy<IMongoCollection<ClientStoreVersionDocument>> _lazyCollection;

        public ClientStoreBaseliner(ISystemClock systemClock, IMongoDatabaseClientProvider clientProvider, MongoDbClientStoreSettings mongoDbClientStoreSettings) {
            if (clientProvider == null) throw new ArgumentNullException(nameof(clientProvider));
            if (mongoDbClientStoreSettings == null) throw new ArgumentNullException(nameof(mongoDbClientStoreSettings));
            
            _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));

            _lazyCollection = new Lazy<IMongoCollection<ClientStoreVersionDocument>>(() => {
                var database = clientProvider.Provide();
                return database.GetCollection<ClientStoreVersionDocument>(mongoDbClientStoreSettings.CollectionName);
            });
        }

        public async Task SetBaseline(IClientStoreMigrationStep step) {
            var versionDoc = new ClientStoreVersionDocument {
                Time = _systemClock.UtcNow,
                Version = step.Version,
                PackageVersion = GetType().Assembly.GetName().Version.ToString(3),
                StepName = step.GetType().Name
            };

            var currentBaseline = await GetBaseline().ConfigureAwait(continueOnCapturedContext: false);
            if (currentBaseline > step.Version) throw new InvalidOperationException($"Cannot set the baseline to '{step.Version}'. There already is a newer version deployed ({currentBaseline}).");
            
            var result = await _lazyCollection.Value.ReplaceOneAsync(
                filter: new JsonFilterDefinition<ClientStoreVersionDocument>("{'_id': '" + ClientStoreVersionDocument.VersionDocumentId + "'}"),
                options: new ReplaceOptions {IsUpsert = true},
                replacement: versionDoc)
                .ConfigureAwait(continueOnCapturedContext: false);

            if (!result.IsAcknowledged) throw new InvalidOperationException("Could not set the new baseline in MongoDb. The operation was not acknowledged.");
        }

        public async Task<int?> GetBaseline() {
            var latestVersion = await _lazyCollection.Value
                .Find(new JsonFilterDefinition<ClientStoreVersionDocument>("{'_id': '" + ClientStoreVersionDocument.VersionDocumentId + "'}"))
                .Sort(new JsonSortDefinition<ClientStoreVersionDocument>("{'version':-1}"))
                .Limit(1)
                .FirstOrDefaultAsync()
                .ConfigureAwait(continueOnCapturedContext: false);

            return latestVersion?.Version;
        }
    }
}