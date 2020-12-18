using System;
using Dalion.HttpMessageSigning.Verification.SqlServer.Infrastructure.Setup.GenericScripts;

namespace Dalion.HttpMessageSigning.Verification.SqlServer.Infrastructure.Setup {
    internal class DatabaseDeleterFactory : IDatabaseDeleterFactory {
        private readonly IGenericSqlScriptsReader _genericSqlScriptsReader;
        private readonly string _masterConnectionString;

        public DatabaseDeleterFactory(string masterConnectionString, IGenericSqlScriptsReader genericSqlScriptsReader) {
            _masterConnectionString = masterConnectionString ?? throw new ArgumentNullException(nameof(masterConnectionString));
            _genericSqlScriptsReader = genericSqlScriptsReader ?? throw new ArgumentNullException(nameof(genericSqlScriptsReader));
        }

        public IDatabaseDeleter CreateForDb(string dbName) {
            if (dbName == null) throw new ArgumentNullException(nameof(dbName));
            return new DatabaseDeleter(_masterConnectionString, dbName, _genericSqlScriptsReader);
        }
    }
}