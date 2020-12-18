using System;

namespace Dalion.HttpMessageSigning.Verification.SqlServer.Infrastructure.Setup.OutdatedDatabases {
    internal class OutdatedDatabaseCleaner : IOutdatedDatabaseCleaner {
        private readonly IDatabaseDeleterFactory _databaseDeleterFactory;
        private readonly IOutdatedDatabaseFinder _outdatedDatabaseFinder;

        public OutdatedDatabaseCleaner(IOutdatedDatabaseFinder outdatedDatabaseFinder, IDatabaseDeleterFactory databaseDeleterFactory) {
            _outdatedDatabaseFinder = outdatedDatabaseFinder ?? throw new ArgumentNullException(nameof(outdatedDatabaseFinder));
            _databaseDeleterFactory = databaseDeleterFactory ?? throw new ArgumentNullException(nameof(databaseDeleterFactory));
        }

        public void CleanOutdatedDatabases() {
            var outdatedDatabaseNames = _outdatedDatabaseFinder.FindOutdatedDatabases();
            foreach (var outdatedDb in outdatedDatabaseNames) {
                var dbDeleter = _databaseDeleterFactory.CreateForDb(outdatedDb);
                dbDeleter.DeleteDatabase();
            }
        }
    }
}