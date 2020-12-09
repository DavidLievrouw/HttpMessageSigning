using FakeItEasy;
using FluentAssertions;
using MongoDB.Driver;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    public class MongoDatabaseClientProviderTests {
        public class ProvideByConnectionString : MongoDatabaseClientProviderTests {
            private readonly MongoDatabaseClientProvider _sut;
            private readonly string _connectionString;

            public ProvideByConnectionString() {
                _connectionString = "mongodb://localhost:27017/Auth";
                _sut = new MongoDatabaseClientProvider(_connectionString);
            }

            [Fact]
            public void ReturnsMongoDatabase() {
                var actual = _sut.Provide();
                actual.Should().NotBeNull().And.BeAssignableTo<IMongoDatabase>();
            }
        }

        public class ProvideByDatabase : MongoDatabaseClientProviderTests {
            private readonly MongoDatabaseClientProvider _sut;
            private readonly IMongoDatabase _db;

            public ProvideByDatabase() {
                _db = A.Fake<IMongoDatabase>();
                _sut = new MongoDatabaseClientProvider(_db);
            }

            [Fact]
            public void ReturnsMongoDatabase() {
                var actual = _sut.Provide();
                actual.Should().NotBeNull().And.BeAssignableTo<IMongoDatabase>();
                actual.Should().Be(_db);
            }
        }
    }
}