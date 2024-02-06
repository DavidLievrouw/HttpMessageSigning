using System;
using EphemeralMongo;

namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    public class MongoDbBuilder : IDisposable {
        private string _databaseName;

        private IMongoRunner _runner;

        public void Dispose() {
            if (_runner == null) return;

            var client = MongoClient.Create(_runner.ConnectionString);
            client.DropDatabase(_databaseName);
            _runner?.Dispose();
        }

        public static MongoDbBuilder New() {
            return new MongoDbBuilder();
        }

        public MongoDbBuilder WithDatabaseName(string databaseName) {
            _databaseName = databaseName;
            return this;
        }

        public string Build() {
            Console.WriteLine($"Starting {nameof(EphemeralMongo)}...");
            _runner = MongoRunner.Run();
            Console.WriteLine($"Running {nameof(EphemeralMongo)} at {_runner.ConnectionString}...");

            return _runner.ConnectionString;
        }
    }
}