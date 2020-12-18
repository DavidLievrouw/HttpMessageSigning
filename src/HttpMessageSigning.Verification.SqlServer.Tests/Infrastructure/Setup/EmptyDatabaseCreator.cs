using System;
using Microsoft.Data.SqlClient;
using Dalion.HttpMessageSigning.Verification.SqlServer.Infrastructure.Setup.GenericScripts;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace Dalion.HttpMessageSigning.Verification.SqlServer.Infrastructure.Setup {
    internal class EmptyDatabaseCreator : IEmptyDatabaseCreator {
        private readonly string _databaseName;
        private readonly IGenericSqlScriptsReader _genericSqlScriptsReader;
        private readonly string _masterConnectionString;

        public EmptyDatabaseCreator(string masterConnectionString, string databaseName, IGenericSqlScriptsReader genericSqlScriptsReader) {
            _masterConnectionString = masterConnectionString ?? throw new ArgumentNullException(nameof(masterConnectionString));
            _databaseName = databaseName ?? throw new ArgumentNullException(nameof(databaseName));
            _genericSqlScriptsReader = genericSqlScriptsReader ?? throw new ArgumentNullException(nameof(genericSqlScriptsReader));
        }

        public void CreateEmptyDatabase() {
            using (var connection = new SqlConnection(_masterConnectionString)) {
                var server = new Server(new ServerConnection(connection));
                var sqlCommand = _genericSqlScriptsReader.ReadCreateEmptyDatabaseSql(_databaseName);
                try {
                    server.ConnectionContext.ExecuteNonQuery(sqlCommand);
                    server.ConnectionContext.CommitTransaction();
                }
                catch (Exception ex) {
                    throw new ExecutionFailureException(
                        $"While creating the empty database, the execution of the following command failed: {sqlCommand}",
                        ex);
                }
            }
        }
    }
}