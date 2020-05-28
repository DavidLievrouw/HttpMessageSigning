using System;

namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    public class MongoSetup : IDisposable {
        private readonly MongoDbBuilder _mongoDbBuilder;

        public MongoSetup() {
            DatabaseName = Guid.NewGuid().ToString();
            _mongoDbBuilder = MongoDbBuilder.New().WithDatabaseName(DatabaseName);
            MongoServerConnectionString = _mongoDbBuilder.Build();
        }

        public string MongoServerConnectionString { get; }
        public string DatabaseName { get; }

        public void Dispose() {
            _mongoDbBuilder?.Dispose();
        }
    }
}