using Xunit;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    [CollectionDefinition(nameof(SqlServerCollection))]
    public class SqlServerCollection : ICollectionFixture<SqlServerFixture> {
        // A class with no code, only used to define the collection
    }
}