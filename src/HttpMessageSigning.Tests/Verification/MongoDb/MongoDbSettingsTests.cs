using System;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    public class MongoDbSettingsTests {
        private readonly MongoDbSettings _sut;

        public MongoDbSettingsTests() {
            _sut = new MongoDbSettings {
                ConnectionString = "mongodb://localhost:27017/?connectTimeoutMS=60000",
                DatabaseName = "DbForTests",
                CollectionName = "signatureclients"
            };
        }

        public class Validate : MongoDbSettingsTests {
            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptyConnectionString_ThrowsValidationException(string nullOrEmpty) {
                _sut.ConnectionString = nullOrEmpty;
                Action act = () => _sut.Validate();
                act.Should().Throw<ValidationException>();
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptyDatabaseName_ThrowsValidationException(string nullOrEmpty) {
                _sut.DatabaseName = nullOrEmpty;
                Action act = () => _sut.Validate();
                act.Should().Throw<ValidationException>();
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptyCollectionName_ThrowsValidationException(string nullOrEmpty) {
                _sut.CollectionName = nullOrEmpty;
                Action act = () => _sut.Validate();
                act.Should().Throw<ValidationException>();
            }

            [Fact]
            public void GivenValidOptions_DoesNotThrow() {
                Action act = () => _sut.Validate();
                act.Should().NotThrow();
            }
        }
    }
}