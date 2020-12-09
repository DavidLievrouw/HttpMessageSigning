using System;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    public class SqlServerClientStoreSettingsTests {
        private readonly SqlServerClientStoreSettings _sut;

        public SqlServerClientStoreSettingsTests() {
            _sut = new SqlServerClientStoreSettings {
                ConnectionString = "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;",
                TableName = "signatureclients",
                ClientCacheEntryExpiration = TimeSpan.FromMinutes(3)
            };
        }

        public class Validate : SqlServerClientStoreSettingsTests {
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