namespace Dalion.HttpMessageSigning.Verification.SqlServer.Infrastructure.Setup {
    internal interface IDatabaseSynchronizer {
        void CleanOutdatedDatabases();
        void CreateDatabaseForTests();
        void DeleteTestDatabase();
    }
}