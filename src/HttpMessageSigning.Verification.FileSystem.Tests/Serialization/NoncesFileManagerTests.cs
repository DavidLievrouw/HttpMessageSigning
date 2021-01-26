using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Dalion.HttpMessageSigning.TestUtils;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.FileSystem.Serialization {
    public class NoncesFileManagerTests {
        private readonly INonceDataRecordSerializer _dataRecordSerializer;
        private readonly string _filePath;
        private readonly IFileReader _fileReader;
        private readonly IFileWriter _fileWriter;
        private readonly string _path;
        private readonly NoncesFileManager _sut;

        public NoncesFileManagerTests() {
            FakeFactory.Create(out _fileReader, out _fileWriter, out _dataRecordSerializer);
            _filePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".xml");
            _sut = new NoncesFileManager(_fileReader, _fileWriter, _filePath, _dataRecordSerializer);
        }

        public class Write : NoncesFileManagerTests {
            private readonly NonceDataRecord[] _nonces;

            public Write() {
                _nonces = new[] {
                    new NonceDataRecord {Value = "n001"},
                    new NonceDataRecord {Value = "n002"}
                };
            }

            [Fact]
            public void GivenNullNonces_ThrowsArgumentNullException() {
                Func<Task> act = () => _sut.Write(nonces: null);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public async Task WritesNoncesToFile() {
                A.CallTo(() => _dataRecordSerializer.Serialize(A<NonceDataRecord>._))
                    .ReturnsLazily((NonceDataRecord c) => new XElement("Nonce", c.Value));

                XDocument writtenDocument = null;
                A.CallTo(() => _fileWriter.Write(_filePath, A<XDocument>._))
                    .Invokes((string f, XDocument xml) => writtenDocument = xml)
                    .Returns(Task.CompletedTask);

                await _sut.Write(_nonces);

                var expected = new XDocument(new XElement(XNamespace.Get("https://dalion.eu/httpmessagesigning") + "Dalion",
                    new XElement("Nonces",
                        new XElement("Nonce", "n001"),
                        new XElement("Nonce", "n002")
                    )));
                writtenDocument.ToString().Should().Be(expected.ToString());
            }

            [Fact]
            public void GivenZeroNonces_DoesNotThrow() {
                Func<Task> act = () => _sut.Write(Enumerable.Empty<NonceDataRecord>());
                act.Should().NotThrow();
            }

            [Fact]
            public void GivenZeroNonces_WritesValidEmptyDocument() {
                XDocument writtenDocument = null;
                A.CallTo(() => _fileWriter.Write(_filePath, A<XDocument>._))
                    .Invokes((string f, XDocument xml) => writtenDocument = xml)
                    .Returns(Task.CompletedTask);

                Func<Task> act = () => _sut.Write(Enumerable.Empty<NonceDataRecord>());
                act.Should().NotThrow();

                var expected = new XDocument(new XElement(XNamespace.Get("https://dalion.eu/httpmessagesigning") + "Dalion",
                    new XElement("Nonces")));
                writtenDocument.ToString().Should().Be(expected.ToString());
            }
        }

        public class Read : NoncesFileManagerTests {
            [Fact]
            public async Task WhenFileDoesNotExist_OrIsEmpty_ReturnsEmpty() {
                A.CallTo(() => _fileReader.Read(_filePath))
                    .Returns(FileReader.EmptyDocument.Value);

                var actual = await _sut.Read();

                actual.Should().NotBeNull().And.BeEmpty();
            }

            [Fact]
            public async Task ReturnsDeserializedNoncesFromFile() {
                var fileContents = new XDocument(new XElement(XNamespace.Get("https://dalion.eu/httpmessagesigning") + "Dalion",
                    new XElement("Nonces",
                        new XElement("Nonce", "n001"),
                        new XElement("Nonce", "n002")
                    )));

                A.CallTo(() => _fileReader.Read(_filePath))
                    .Returns(fileContents);

                A.CallTo(() => _dataRecordSerializer.Deserialize(A<XContainer>._))
                    .ReturnsLazily((XContainer x) => new NonceDataRecord {Value = x.FirstNode.As<XText>().Value});

                var actual = await _sut.Read();

                var expected = new[] {
                    new NonceDataRecord {Value = "n001"},
                    new NonceDataRecord {Value = "n002"}
                };
                actual.Should().BeEquivalentTo<NonceDataRecord>(expected);
            }
        }
    }
}