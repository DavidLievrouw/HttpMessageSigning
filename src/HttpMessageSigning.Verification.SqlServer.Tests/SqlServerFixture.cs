using System;
using Dalion.HttpMessageSigning.Verification.SqlServer.Infrastructure;
using Dalion.HttpMessageSigning.Verification.SqlServer.Infrastructure.Setup;
using Dalion.HttpMessageSigning.Verification.SqlServer.Infrastructure.Setup.GenericScripts;
using Dalion.HttpMessageSigning.Verification.SqlServer.Infrastructure.Setup.OutdatedDatabases;
using Dalion.HttpMessageSigning.Verification.SqlServer.Infrastructure.Setup.Schema;
using Microsoft.Extensions.Configuration;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    public class SqlServerFixture : IDisposable {
        private static readonly IDatabaseSynchronizer DbSynchronizer;

        static SqlServerFixture() {
            var config = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();
            SqlServerConfig = SqlServerConfigReader.Read(config);

            var genericScriptsReader = new GenericSqlScriptsReader(new GenericScriptTemplateReader());
            var databaseDeleterFactory = new DatabaseDeleterFactory(SqlServerConfig.GetConnectionStringForMasterDatabase(), genericScriptsReader);
            var systemClock = new RealSystemClock();

            DbSynchronizer = new DatabaseSynchronizer(
                new OutdatedDatabaseCleaner(
                    new OutdatedDatabaseFinder(
                        SqlServerConfig.GetConnectionStringForMasterDatabase(),
                        SqlServerConfig.DatabaseNameTemplate,
                        systemClock),
                    databaseDeleterFactory),
                new EmptyDatabaseCreator(
                    SqlServerConfig.GetConnectionStringForMasterDatabase(),
                    SqlServerConfig.GetUniqueDatabaseName(),
                    genericScriptsReader),
                new DatabaseSchemaApplier(
                    SqlServerConfig.GetConnectionStringForTestDatabase(),
                    new DatabaseScriptsReader(@"Infrastructure\Setup\Scripts")),
                databaseDeleterFactory.CreateForDb(SqlServerConfig.GetUniqueDatabaseName()));
        }

        public SqlServerFixture() {
            try {
                DbSynchronizer.CleanOutdatedDatabases();
                DbSynchronizer.DeleteTestDatabase();
                DbSynchronizer.CreateDatabaseForTests();
            }
            catch (Exception) {
                Dispose();
                throw;
            }
        }

        internal static SqlServerConfig SqlServerConfig { get; }

        public void Dispose() {
            DbSynchronizer.DeleteTestDatabase();
        }
    }
}