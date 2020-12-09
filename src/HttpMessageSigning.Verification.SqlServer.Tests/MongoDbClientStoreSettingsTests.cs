using System;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    public class MongoDbClientStoreSettingsTests {
        private readonly MongoDbClientStoreSettings _sut;

        public MongoDbClientStoreSettingsTests() {
            _sut = new MongoDbClientStoreSettings {
                ConnectionString = "mongodb://localhost:27017/DbForTests?connectTimeoutMS=60000",
                CollectionName = "signatureclients",
                ClientCacheEntryExpiration = TimeSpan.FromMinutes(3)
            };
        }

        public class Validate : MongoDbClientStoreSettingsTests {
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
            public void GivenNullOrEmptyCollectionName_ThrowsValidationException(string nullOrEmpty) {
                _sut.CollectionName = nullOrEmpty;
                Action act = () => _sut.Validate();
                act.Should().Throw<ValidationException>();
            }

            [Theory]
            [InlineData(0)]
            [InlineData(-1)]
            [InlineData(-99)]
            public void GivenZeroOrNegativeCacheEntryExpiration_DoesNotThrow(int expirationSeconds) {
                _sut.ClientCacheEntryExpiration = TimeSpan.FromSeconds(expirationSeconds);
                Action act = () => _sut.Validate();
                act.Should().NotThrow();
            }
            
            [Fact]
            public void GivenValidOptions_DoesNotThrow() {
                Action act = () => _sut.Validate();
                act.Should().NotThrow();
            }
        }
    }
}