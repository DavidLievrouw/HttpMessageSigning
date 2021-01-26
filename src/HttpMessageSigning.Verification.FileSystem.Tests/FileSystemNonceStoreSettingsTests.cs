using System;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.FileSystem {
    public class FileSystemNonceStoreSettingsTests {
        private readonly FileSystemNonceStoreSettings _sut;

        public FileSystemNonceStoreSettingsTests() {
            _sut = new FileSystemNonceStoreSettings {
                FilePath = "C:\\ProgramData\\Nonces.xml"
            };
        }

        public class Validate : FileSystemNonceStoreSettingsTests {
            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptyFilePath_ThrowsValidationException(string nullOrEmpty) {
                _sut.FilePath = nullOrEmpty;
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