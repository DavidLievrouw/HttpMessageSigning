using System;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Dalion.HttpMessageSigning.Verification.MongoDb.Migrations {
    internal class Baseliner : IBaseliner {
        private readonly ISystemClock _systemClock;
        private readonly Lazy<IMongoCollection<VersionDocument>> _lazyCollection;

        public Baseliner(ISystemClock systemClock, IMongoDatabaseClientProvider clientProvider, MongoDbClientStoreSettings mongoDbClientStoreSettings) {
            if (clientProvider == null) throw new ArgumentNullException(nameof(clientProvider));
            if (mongoDbClientStoreSettings == null) throw new ArgumentNullException(nameof(mongoDbClientStoreSettings));
            
            _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));

            _lazyCollection = new Lazy<IMongoCollection<VersionDocument>>(() => {
                var database = clientProvider.Provide();
                return database.GetCollection<VersionDocument>(mongoDbClientStoreSettings.CollectionName);
            });
        }

        public async Task SetBaseline(IMigrationStep step) {
            var versionDoc = new VersionDocument {
                Time = _systemClock.UtcNow,
                Version = step.Version,
                PackageVersion = GetType().Assembly.GetName().Version.ToString(3),
                StepName = step.GetType().Name
            };

            var currentBaseline = await GetBaseline();
            if (currentBaseline > step.Version) throw new InvalidOperationException($"Cannot set the baseline to '{step.Version}'. There already is a newer version deployed ({currentBaseline}).");
            
            var result = await _lazyCollection.Value.ReplaceOneAsync(
                filter: new JsonFilterDefinition<VersionDocument>("{'_id': '" + versionDoc.Id + "'}"),
                options: new ReplaceOptions {IsUpsert = true},
                replacement: versionDoc);

            if (!result.IsAcknowledged) throw new InvalidOperationException("Could not set the new baseline in MongoDb. The operation was not acknowledged.");
        }

        public async Task<int?> GetBaseline() {
            var latestVersion = await _lazyCollection.Value
                .Find("{}")
                .Sort(new JsonSortDefinition<VersionDocument>("{'version':-1}"))
                .Limit(1)
                .FirstOrDefaultAsync();

            return latestVersion?.Version;
        }
    }
}