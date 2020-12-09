using System;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    public class MongoDbNonceStoreSettingsTests {
        private readonly MongoDbNonceStoreSettings _sut;

        public MongoDbNonceStoreSettingsTests() {
            _sut = new MongoDbNonceStoreSettings {
                ConnectionString = "mongodb://localhost:27017/DbForTests?connectTimeoutMS=60000",
                CollectionName = "signaturenonces"
            };
        }

        public class Validate : MongoDbNonceStoreSettingsTests {
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
            
            [Fact]
            public void GivenValidOptions_DoesNotThrow() {
                Action act = () => _sut.Validate();
                act.Should().NotThrow();
            }
        }
    }
}