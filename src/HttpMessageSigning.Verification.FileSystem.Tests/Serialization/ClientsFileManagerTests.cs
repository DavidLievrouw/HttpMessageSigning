using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Dalion.HttpMessageSigning.TestUtils;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.FileSystem.Serialization {
    public class ClientsFileManagerTests : IDisposable {
        private readonly IClientDataRecordSerializer _clientDataRecordSerializer;
        private readonly string _filePath;
        private readonly IFileReader _fileReader;
        private readonly IFileWriter _fileWriter;
        private readonly string _path;
        private readonly SemaphoreSlim _semaphore;
        private readonly ISemaphoreFactory _semaphoreFactory;
        private readonly ClientsFileManager _sut;

        public ClientsFileManagerTests() {
            FakeFactory.Create(out _fileReader, out _fileWriter, out _semaphoreFactory, out _clientDataRecordSerializer);
            _semaphore = new SemaphoreSlim(1, 1);
            A.CallTo(() => _semaphoreFactory.CreateLock())
                .Returns(_semaphore);
            _filePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".xml");
            _sut = new ClientsFileManager(_fileReader, _fileWriter, _filePath, _semaphoreFactory, _clientDataRecordSerializer);
        }

        public void Dispose() {
            _sut?.Dispose();
            _semaphore?.Dispose();
        }

        public class Write : ClientsFileManagerTests {
            private readonly ClientDataRecord[] _clients;

            public Write() {
                _clients = new[] {
                    new ClientDataRecord {Id = "client001"},
                    new ClientDataRecord {Id = "client002"}
                };
            }

            [Fact]
            public void GivenNullClients_ThrowsArgumentNullException() {
                Func<Task> act = () => _sut.Write(clients: null);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public async Task WritesClientsToFile() {
                A.CallTo(() => _clientDataRecordSerializer.Serialize(A<ClientDataRecord>._))
                    .ReturnsLazily((ClientDataRecord c) => new XElement("Client", c.Id));

                XDocument writtenDocument = null;
                A.CallTo(() => _fileWriter.Write(_filePath, A<XDocument>._))
                    .Invokes((string f, XDocument xml) => writtenDocument = xml)
                    .Returns(Task.CompletedTask);

                await _sut.Write(_clients);

                var expected = new XDocument(new XElement(XNamespace.Get("https://dalion.eu/httpmessagesigning") + "Dalion",
                    new XElement("Clients",
                        new XElement("Client", "client001"),
                        new XElement("Client", "client002")
                    )));
                writtenDocument.ToString().Should().Be(expected.ToString());
            }

            [Fact]
            public void GivenZeroClients_DoesNotThrow() {
                Func<Task> act = () => _sut.Write(Enumerable.Empty<ClientDataRecord>());
                act.Should().NotThrow();
            }

            [Fact]
            public void GivenZeroClients_WritesValidEmptyDocument() {
                XDocument writtenDocument = null;
                A.CallTo(() => _fileWriter.Write(_filePath, A<XDocument>._))
                    .Invokes((string f, XDocument xml) => writtenDocument = xml)
                    .Returns(Task.CompletedTask);

                Func<Task> act = () => _sut.Write(Enumerable.Empty<ClientDataRecord>());
                act.Should().NotThrow();

                var expected = new XDocument(new XElement(XNamespace.Get("https://dalion.eu/httpmessagesigning") + "Dalion",
                    new XElement("Clients")));
                writtenDocument.ToString().Should().Be(expected.ToString());
            }
        }

        public class Read : ClientsFileManagerTests {
            [Fact]
            public async Task WhenFileDoesNotExist_OrIsEmpty_ReturnsEmpty() {
                A.CallTo(() => _fileReader.Read(_filePath))
                    .Returns(FileReader.EmptyDocument.Value);

                var actual = await _sut.Read();

                actual.Should().NotBeNull().And.BeEmpty();
            }

            [Fact]
            public async Task ReturnsDeserializedClientsFromFile() {
                var fileContents = new XDocument(new XElement(XNamespace.Get("https://dalion.eu/httpmessagesigning") + "Dalion",
                    new XElement("Clients",
                        new XElement("Client", "client001"),
                        new XElement("Client", "client002")
                    )));

                A.CallTo(() => _fileReader.Read(_filePath))
                    .Returns(fileContents);

                A.CallTo(() => _clientDataRecordSerializer.Deserialize(A<XContainer>._))
                    .ReturnsLazily((XContainer x) => new ClientDataRecord {Id = x.FirstNode.As<XText>().Value});

                var actual = await _sut.Read();

                var expected = new[] {
                    new ClientDataRecord {Id = "client001"},
                    new ClientDataRecord {Id = "client002"}
                };
                actual.Should().BeEquivalentTo<ClientDataRecord>(expected);
            }
        }
    }
}