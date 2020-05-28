using Xunit;

namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    [CollectionDefinition("MongoDbCollection")]
    public class MongoDbCollection : ICollectionFixture<MongoSetup> {
        // A class with no code, only used to define the collection
    }
}