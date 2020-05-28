using MongoDB.Driver;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    [Collection("MongoDbCollection")]
    public class MongoIntegrationTest {
        protected readonly IMongoDatabase Database;

        public MongoIntegrationTest(MongoSetup mongoSetup) {
            var client = MongoClient.Create(mongoSetup.MongoServerConnectionString);
            Database = client.GetDatabase(mongoSetup.DatabaseName);
        }
    }
}