using System;
using Dalion.HttpMessageSigning.Verification.SqlServer.Infrastructure.Setup.OutdatedDatabases;
using Dalion.HttpMessageSigning.Verification.SqlServer.Infrastructure.Setup.Schema;

namespace Dalion.HttpMessageSigning.Verification.SqlServer.Infrastructure.Setup {
    internal class DatabaseSynchronizer : IDatabaseSynchronizer {
        private readonly IDatabaseDeleter _databaseDeleter;
        private readonly IDatabaseSchemaApplier _databaseSchemaApplier;
        private readonly IEmptyDatabaseCreator _emptyDatabaseCreator;
        private readonly IOutdatedDatabaseCleaner _outdatedDatabaseCleaner;

        public DatabaseSynchronizer(
            IOutdatedDatabaseCleaner outdatedDatabaseCleaner,
            IEmptyDatabaseCreator emptyDatabaseCreator,
            IDatabaseSchemaApplier databaseSchemaApplier,
            IDatabaseDeleter databaseDeleter) {
            _outdatedDatabaseCleaner = outdatedDatabaseCleaner ?? throw new ArgumentNullException(nameof(outdatedDatabaseCleaner));
            _emptyDatabaseCreator = emptyDatabaseCreator ?? throw new ArgumentNullException(nameof(emptyDatabaseCreator));
            _databaseSchemaApplier = databaseSchemaApplier ?? throw new ArgumentNullException(nameof(databaseSchemaApplier));
            _databaseDeleter = databaseDeleter ?? throw new ArgumentNullException(nameof(databaseDeleter));
        }

        public void CleanOutdatedDatabases() {
            _outdatedDatabaseCleaner.CleanOutdatedDatabases();
        }

        public void CreateDatabaseForTests() {
            _emptyDatabaseCreator.CreateEmptyDatabase();
            _databaseSchemaApplier.ApplySchema();
        }

        public void DeleteTestDatabase() {
            _databaseDeleter.DeleteDatabase();
        }
    }
}