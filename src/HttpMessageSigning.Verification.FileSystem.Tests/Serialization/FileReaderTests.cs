using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.FileSystem.Serialization {
    public class FileReaderTests {
        private readonly FileReader _sut;

        public FileReaderTests() {
            _sut = new FileReader();
        }

        public class Read : FileReaderTests {
            private readonly string _tempPath;
            private readonly string _tempFileName;

            public Read() {
                _tempPath = Path.GetTempPath();
                _tempFileName = Guid.NewGuid() + ".xml";
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptyFilePath_ThrowsArgumentException(string nullOrEmpty) {
                Func<Task> act = () => _sut.Read(nullOrEmpty);

                act.Should().Throw<ArgumentException>();
            }

            [Fact]
            public async Task GivenInvalidPath_ReturnsEmptyDocument() {
                var invalidPath = ":something_invalid:";

                var actual = await _sut.Read(invalidPath);

                actual.Should().NotBeNull();
                actual.Root.Name.Should().Be(XName.Get("Dalion", "https://dalion.eu/httpmessagesigning"));
                actual.Root.Elements().Should().BeEmpty();
            }

            [Fact]
            public async Task WhenFileDoesNotExist_ReturnsEmptyDocument() {
                var filePath = Path.Combine(_tempPath, _tempFileName);

                var actual = await _sut.Read(filePath);

                actual.Should().NotBeNull();
                actual.Root.Name.Should().Be(XName.Get("Dalion", "https://dalion.eu/httpmessagesigning"));
                actual.Root.Elements().Should().BeEmpty();
            }

            [Fact]
            public async Task WhenDirectoryDoesNotExist_ReturnsEmptyDocument() {
                var filePath = Path.Combine(_tempPath, "subdir" + Guid.NewGuid(), _tempFileName);

                var actual = await _sut.Read(filePath);

                actual.Should().NotBeNull();
                actual.Root.Name.Should().Be(XName.Get("Dalion", "https://dalion.eu/httpmessagesigning"));
                actual.Root.Elements().Should().BeEmpty();
            }

            [Fact]
            public async Task ReadsXmlDataFromFile() {
                var data = new XDocument(
                    new XElement("Root",
                        new XElement("Data", "the_value")));
                var filePath = Path.Combine(_tempPath, _tempFileName);
                await new FileWriter().Write(filePath, data);

                var actual = await _sut.Read(filePath);

                var isEqual = XNode.DeepEquals(actual, data);
                isEqual.Should().BeTrue();
            }
        }

        public class FileExists : FileReaderTests {
            private readonly string _tempPath;
            private readonly string _tempFileName;

            public FileExists() {
                _tempPath = Path.GetTempPath();
                _tempFileName = Guid.NewGuid() + ".xml";
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptyFilePath_ThrowsArgumentException(string nullOrEmpty) {
                Action act = () => _sut.FileExists(nullOrEmpty);

                act.Should().Throw<ArgumentException>();
            }

            [Fact]
            public void GivenInvalidPath_ReturnsFalse() {
                var invalidPath = ":something_invalid:";

                var actual = _sut.FileExists(invalidPath);

                actual.Should().BeFalse();
            }

            [Fact]
            public void WhenFileDoesNotExist_ReturnsEmptyDocument() {
                var filePath = Path.Combine(_tempPath, _tempFileName);

                var actual = _sut.FileExists(filePath);

                actual.Should().BeFalse();
            }

            [Fact]
            public void WhenDirectoryDoesNotExist_ReturnsEmptyDocument() {
                var filePath = Path.Combine(_tempPath, "subdir" + Guid.NewGuid(), _tempFileName);

                var actual = _sut.FileExists(filePath);

                actual.Should().BeFalse();
            }

            [Fact]
            public async Task WhenFileExists_ReturnsTrue() {
                var data = new XDocument(
                    new XElement("Root",
                        new XElement("Data", "the_value")));
                var filePath = Path.Combine(_tempPath, _tempFileName);
                await new FileWriter().Write(filePath, data);

                var actual = _sut.FileExists(filePath);

                actual.Should().BeTrue();
            }
        }
    }
}