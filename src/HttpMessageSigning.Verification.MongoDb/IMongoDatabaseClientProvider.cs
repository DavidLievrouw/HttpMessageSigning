using MongoDB.Driver;

 namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    internal interface IMongoDatabaseClientProvider {
        IMongoDatabase Provide();
    }
}