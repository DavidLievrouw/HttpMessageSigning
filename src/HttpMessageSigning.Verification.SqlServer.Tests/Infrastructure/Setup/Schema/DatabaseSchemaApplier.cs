using System;
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace Dalion.HttpMessageSigning.Verification.SqlServer.Infrastructure.Setup.Schema {
    internal class DatabaseSchemaApplier : IDatabaseSchemaApplier {
        private readonly string _connectionString;
        private readonly IDatabaseScriptsReader _databaseScriptsReader;

        public DatabaseSchemaApplier(string connectionString, IDatabaseScriptsReader databaseScriptsReader) {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _databaseScriptsReader = databaseScriptsReader ?? throw new ArgumentNullException(nameof(databaseScriptsReader));
        }

        public void ApplySchema() {
            using (var connection = new SqlConnection(_connectionString)) {
                var server = new Server(new ServerConnection(connection));
                foreach (var sqlCommand in _databaseScriptsReader.ReadAllDatabaseScriptsInOrder()) {
                    try {
                        server.ConnectionContext.ExecuteNonQuery(sqlCommand);
                        server.ConnectionContext.CommitTransaction();
                    }
                    catch (Exception ex) {
                        throw new ExecutionFailureException(
                            $"While synchronizing the database, the execution of the following command failed: {sqlCommand}",
                            ex);
                    }
                }
            }
        }
    }
}