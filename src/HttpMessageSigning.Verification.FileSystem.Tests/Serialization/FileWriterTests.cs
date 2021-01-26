using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.FileSystem.Serialization {
    public class FileWriterTests {
        private readonly FileWriter _sut;

        public FileWriterTests() {
            _sut = new FileWriter();
        }

        public class Write : FileWriterTests {
            private readonly XDocument _data;
            private readonly string _tempPath;
            private readonly string _tempFileName;

            public Write() {
                _data = new XDocument(
                    new XElement("Root",
                        new XElement("Data", "the_value")));
                _tempPath = Path.GetTempPath();
                _tempFileName = Guid.NewGuid() + ".xml";
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptyFilePath_ThrowsArgumentException(string nullOrEmpty) {
                Func<Task> act = () => _sut.Write(nullOrEmpty, _data);

                act.Should().Throw<ArgumentException>();
            }

            [Fact]
            public void GivenInvalidPath_ThrowsArgumentException() {
                var invalidPath = ":something_invalid:";

                Func<Task> act = () => _sut.Write(invalidPath, _data);

                act.Should().Throw<ArgumentException>();
            }

            [Fact]
            public async Task WhenDirectoryDoesNotExist_CreatesDirectory() {
                var filePath = Path.Combine(_tempPath, "subdir" + Guid.NewGuid(), _tempFileName);

                try {
                    await _sut.Write(filePath, _data);

                    File.Exists(filePath).Should().BeTrue();
                }
                finally {
                    File.Delete(filePath);
                }
            }

            [Fact]
            public async Task WhenDirectoryDoesNotExist_CreatesDirectoryNested() {
                var filePath = Path.Combine(_tempPath, "subdir1_" + Guid.NewGuid(), "subdir2_" + Guid.NewGuid(), _tempFileName);

                try {
                    await _sut.Write(filePath, _data);

                    File.Exists(filePath).Should().BeTrue();
                }
                finally {
                    File.Delete(filePath);
                }
            }

            [Fact]
            public async Task SavesSpecifiedDataToFile() {
                var filePath = Path.Combine(_tempPath, _tempFileName);

                try {
                    await _sut.Write(filePath, _data);

                    File.Exists(filePath).Should().BeTrue();
                    // ReSharper disable once MethodHasAsyncOverload
                    var actualText = File.ReadAllText(filePath);
                    actualText.Should().Be("<?xml version=\"1.0\" encoding=\"utf-8\"?><Root><Data>the_value</Data></Root>");
                }
                finally {
                    File.Delete(filePath);
                }
            }
        }
    }
}