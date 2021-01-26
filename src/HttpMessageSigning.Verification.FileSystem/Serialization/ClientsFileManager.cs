using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Dalion.HttpMessageSigning.Verification.FileSystem.Serialization {
    internal class ClientsFileManager : IFileManager<ClientDataRecord> {
        private readonly IClientDataRecordSerializer _clientDataRecordSerializer;
        private readonly IFileReader _fileReader;
        private readonly IFileWriter _fileWriter;
        private readonly string _path;

        public ClientsFileManager(IFileReader fileReader, IFileWriter fileWriter, string path, IClientDataRecordSerializer clientDataRecordSerializer) {
            if (string.IsNullOrEmpty(path)) throw new ArgumentException("Value cannot be null or empty.", nameof(path));
            _fileReader = fileReader ?? throw new ArgumentNullException(nameof(fileReader));
            _fileWriter = fileWriter ?? throw new ArgumentNullException(nameof(fileWriter));
            _path = path;
            _clientDataRecordSerializer = clientDataRecordSerializer ?? throw new ArgumentNullException(nameof(clientDataRecordSerializer));
        }

        public Task Write(IEnumerable<ClientDataRecord> clients) {
            if (clients == null) throw new ArgumentNullException(nameof(clients));

            var xmlRoot = new XElement(XName.Get(Constants.XmlNames.Root, Constants.XmlNames.Ns));
            xmlRoot.SetAttributeValue("xmlns", Constants.XmlNames.Ns);
            var clientsElem = new XElement(Constants.XmlNames.ClientsElem);
            foreach (var client in clients) {
                var serialized = _clientDataRecordSerializer.Serialize(client);
                clientsElem.Add(serialized);
            }

            xmlRoot.Add(clientsElem);
            var xmlDocument = new XDocument(xmlRoot);
            return _fileWriter.Write(_path, xmlDocument);
        }

        public async Task<IEnumerable<ClientDataRecord>> Read() {
            var xmlDocument = await _fileReader.Read(_path);
            var clientElems = xmlDocument.Root?.HasElements ?? false
                ? xmlDocument.Root.Element(Constants.XmlNames.ClientsElem)?.Elements() ?? Enumerable.Empty<XElement>()
                : Enumerable.Empty<XElement>();

            var clients = new List<ClientDataRecord>();

            foreach (var clientElem in clientElems) {
                var clientDataRecord = _clientDataRecordSerializer.Deserialize(clientElem);
                clients.Add(clientDataRecord);
            }

            return clients;
        }
    }
}