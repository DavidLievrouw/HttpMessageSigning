namespace Dalion.HttpMessageSigning.Verification.SqlServer.Infrastructure.Setup.GenericScripts {
    internal interface IGenericSqlScriptsReader {
        string ReadCreateEmptyDatabaseSql(string databaseName);
        string ReadCloseConnectionsAndDeleteDatabaseSql(string databaseName);
    }
}