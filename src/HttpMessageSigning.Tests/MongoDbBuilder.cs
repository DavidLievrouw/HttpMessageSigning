using System;
using System.Diagnostics.CodeAnalysis;
using Mongo2Go;

namespace Dalion.HttpMessageSigning {
    [ExcludeFromCodeCoverage]
    public class MongoDbBuilder : IDisposable {
        private string _databaseName;

        private MongoDbRunner _runner;

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
            Console.WriteLine($"Starting {nameof(Mongo2Go)}...");
            _runner = MongoDbRunner.Start();
            Console.WriteLine($"Running {nameof(Mongo2Go)} at {_runner.ConnectionString}...");

            return _runner.ConnectionString;
        }
    }
}