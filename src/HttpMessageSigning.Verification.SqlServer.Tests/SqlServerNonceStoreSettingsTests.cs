using System;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    public class SqlServerNonceStoreSettingsTests {
        private readonly SqlServerNonceStoreSettings _sut;

        public SqlServerNonceStoreSettingsTests() {
            _sut = new SqlServerNonceStoreSettings {
                ConnectionString = "mongodb://localhost:27017/DbForTests?connectTimeoutMS=60000",
                TableName = "signaturenonces"
            };
        }

        public class Validate : SqlServerNonceStoreSettingsTests {
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
                _sut.TableName = nullOrEmpty;
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