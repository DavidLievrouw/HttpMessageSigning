using System;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    public class SqlServerNonceStoreSettingsTests {
        private readonly SqlServerNonceStoreSettings _sut;

        public SqlServerNonceStoreSettingsTests() {
            _sut = new SqlServerNonceStoreSettings {
                ConnectionString = "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;",
                NonceTableName = "signaturenonces",
                VersionTableName = "nonceversions",
                ExpiredNoncesCleanUpInterval = TimeSpan.FromMinutes(1)
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
            public void GivenNullOrEmptyNonceTableName_ThrowsValidationException(string nullOrEmpty) {
                _sut.NonceTableName = nullOrEmpty;
                Action act = () => _sut.Validate();
                act.Should().Throw<ValidationException>();
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptyVersionTableName_ThrowsValidationException(string nullOrEmpty) {
                _sut.VersionTableName = nullOrEmpty;
                Action act = () => _sut.Validate();
                act.Should().Throw<ValidationException>();
            }

            [Theory]
            [InlineData(0)]
            [InlineData(-1)]
            [InlineData(-99)]
            public void GivenZeroOrNegativeExpiredNoncesCleanUpInterval_ThrowsValidationException(int seconds) {
                _sut.ExpiredNoncesCleanUpInterval = TimeSpan.FromSeconds(seconds);
                Action act = () => _sut.Validate();
                act.Should().Throw<ValidationException>();
            }
            
            [Fact]
            public void GivenValidSettings_DoesNotThrow() {
                Action act = () => _sut.Validate();
                act.Should().NotThrow();
            }
        }
    }
}