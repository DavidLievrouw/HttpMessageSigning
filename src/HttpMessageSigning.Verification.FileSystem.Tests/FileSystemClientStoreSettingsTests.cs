using System;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.FileSystem {
    public class FileSystemClientStoreSettingsTests {
        private readonly FileSystemClientStoreSettings _sut;

        public FileSystemClientStoreSettingsTests() {
            _sut = new FileSystemClientStoreSettings {
                FilePath = "C:\\ProgramData\\Clients.xml",
                ClientCacheEntryExpiration = TimeSpan.FromMinutes(3)
            };
        }

        public class Validate : FileSystemClientStoreSettingsTests {
            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptyConnectionString_ThrowsValidationException(string nullOrEmpty) {
                _sut.FilePath = nullOrEmpty;
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