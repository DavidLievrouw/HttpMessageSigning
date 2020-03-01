using MongoDB.Driver;
using Xunit;

namespace Dalion.HttpMessageSigning {
    [Collection("MongoDbCollection")]
    public class MongoIntegrationTest {
        protected IMongoDatabase Database;

        public MongoIntegrationTest(MongoSetup mongoSetup) {
            var client = MongoClient.Create(mongoSetup.MongoServerConnectionString);
            Database = client.GetDatabase(mongoSetup.DatabaseName);
        }
    }
}