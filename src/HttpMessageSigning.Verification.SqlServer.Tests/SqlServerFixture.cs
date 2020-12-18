using System;
using Dalion.HttpMessageSigning.Verification.SqlServer.Infrastructure;
using Dalion.HttpMessageSigning.Verification.SqlServer.Infrastructure.Setup;
using Dalion.HttpMessageSigning.Verification.SqlServer.Infrastructure.Setup.GenericScripts;
using Dalion.HttpMessageSigning.Verification.SqlServer.Infrastructure.Setup.OutdatedDatabases;
using Dalion.HttpMessageSigning.Verification.SqlServer.Infrastructure.Setup.Schema;
using Microsoft.Extensions.Configuration;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    public class SqlServerFixture : IDisposable {
        private readonly IDatabaseSynchronizer _dbSynchronizer;
        
        public SqlServerFixture() {
            var config = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();
            SqlServerConfig = SqlServerConfigReader.Read(config);

            var genericScriptsReader = new GenericSqlScriptsReader(new GenericScriptTemplateReader());
            var databaseDeleterFactory = new DatabaseDeleterFactory(SqlServerConfig.GetConnectionStringForMasterDatabase(), genericScriptsReader);
            var systemClock = new RealSystemClock();

            _dbSynchronizer = new DatabaseSynchronizer(
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
                    new DatabaseScriptsReader()),
                databaseDeleterFactory.CreateForDb(SqlServerConfig.GetUniqueDatabaseName()));
            
            try {
                _dbSynchronizer.CleanOutdatedDatabases();
                _dbSynchronizer.DeleteTestDatabase();
                _dbSynchronizer.CreateDatabaseForTests();
            }
            catch (Exception) {
                _dbSynchronizer.DeleteTestDatabase();
                throw;
            }
        }

        internal SqlServerConfig SqlServerConfig { get; }

        public virtual void Dispose() {
            _dbSynchronizer.DeleteTestDatabase();
        }
    }
}