using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace Dalion.HttpMessageSigning.Verification.SqlServer.Infrastructure.Setup.OutdatedDatabases {
    internal class OutdatedDatabaseFinder : IOutdatedDatabaseFinder {
        private const string ListDatabasesCommandSql = @"SELECT [name] FROM [master].dbo.sysdatabases";
        private readonly string _databaseNameTemplate;

        private readonly string _masterConnectionString;
        private readonly ISystemClock _systemClock;

        public OutdatedDatabaseFinder(string masterConnectionString, string databaseNameTemplate, ISystemClock systemClock) {
            _masterConnectionString = masterConnectionString ?? throw new ArgumentNullException(nameof(masterConnectionString));
            _databaseNameTemplate = databaseNameTemplate ?? throw new ArgumentNullException(nameof(databaseNameTemplate));
            _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
        }

        public IEnumerable<string> FindOutdatedDatabases() {
            var yesterday = _systemClock.UtcNow.LocalDateTime.AddDays(-1).Date;
            return ListDatabases()
                .Select(db => {
                    var isValid = UniqueDbName.TryParse(db, _databaseNameTemplate, out var uniqueDbName);
                    return new {
                        IsValid = isValid,
                        Creation = isValid ? uniqueDbName.CreationDate : new DateTime?(),
                        DbName = db
                    };
                })
                .Where(match => match.IsValid)
                .Select(match => new {
                    DbName = match.DbName,
                    Creation = match.Creation
                })
                .Where(item => item.Creation.Value.Date < yesterday)
                .Select(item => item.DbName);
        }

        private IEnumerable<string> ListDatabases() {
            using (var connection = new SqlConnection(_masterConnectionString)) {
                var server = new Server(new ServerConnection(connection));
                DataSet dataSet;
                try {
                    dataSet = server.ConnectionContext.ExecuteWithResults(ListDatabasesCommandSql);
                }
                catch (Exception ex) {
                    throw new ExecutionFailureException(
                        $"While listing databases, the execution of the following command failed: {ListDatabasesCommandSql}",
                        ex);
                }

                foreach (DataRow dataRow in dataSet.Tables[0].Rows) {
                    yield return (string) dataRow[0];
                }
            }
        }
    }
}