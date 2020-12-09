using MongoDB.Driver;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    internal interface IMongoDatabaseClientProvider {
        IMongoDatabase Provide();
    }
}