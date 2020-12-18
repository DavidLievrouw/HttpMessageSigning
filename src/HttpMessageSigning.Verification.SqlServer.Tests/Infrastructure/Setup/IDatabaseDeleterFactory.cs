namespace Dalion.HttpMessageSigning.Verification.SqlServer.Infrastructure.Setup {
    internal interface IDatabaseDeleterFactory {
        IDatabaseDeleter CreateForDb(string dbName);
    }
}