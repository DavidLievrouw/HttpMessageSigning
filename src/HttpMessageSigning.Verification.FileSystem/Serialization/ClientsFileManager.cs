using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Dalion.HttpMessageSigning.Verification.FileSystem.Serialization {
    internal class ClientsFileManager : IFileManager<ClientDataRecord>, IDisposable {
        private readonly IFileReader _fileReader;
        private readonly IFileWriter _fileWriter;
        private readonly string _path;
        private readonly IClientDataRecordSerializer _clientDataRecordSerializer;
        private readonly SemaphoreSlim _semaphore;
        
        private static readonly TimeSpan MaxLockWaitTime = TimeSpan.FromSeconds(1);

        public ClientsFileManager(IFileReader fileReader, IFileWriter fileWriter, string path, ISemaphoreFactory semaphoreFactory, IClientDataRecordSerializer clientDataRecordSerializer) {
            if (semaphoreFactory == null) throw new ArgumentNullException(nameof(semaphoreFactory));
            if (string.IsNullOrEmpty(path)) throw new ArgumentException("Value cannot be null or empty.", nameof(path));
            _fileReader = fileReader ?? throw new ArgumentNullException(nameof(fileReader));
            _fileWriter = fileWriter ?? throw new ArgumentNullException(nameof(fileWriter));
            _path = path;
            _clientDataRecordSerializer = clientDataRecordSerializer ?? throw new ArgumentNullException(nameof(clientDataRecordSerializer));
            _semaphore = semaphoreFactory.CreateLock();
        }

        public async Task Write(IEnumerable<ClientDataRecord> clients) {
            if (clients == null) throw new ArgumentNullException(nameof(clients));
            
            await _semaphore.WaitAsync(MaxLockWaitTime, CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);

            try {
                var xmlRoot = new XElement(XName.Get(Constants.XmlNames.Root, Constants.XmlNames.Ns));
                xmlRoot.SetAttributeValue("xmlns", Constants.XmlNames.Ns);
                var clientsElem = new XElement(Constants.XmlNames.Elem);
                foreach (var client in clients) {
                    var serialized = _clientDataRecordSerializer.Serialize(client);
                    clientsElem.Add(serialized);
                }
                xmlRoot.Add(clientsElem);
                var xmlDocument = new XDocument(xmlRoot);
                await _fileWriter.Write(_path, xmlDocument);
            }
            finally {
                _semaphore.Release();
            }
        }

        public async Task<IEnumerable<ClientDataRecord>> Read() {
            await _semaphore.WaitAsync(MaxLockWaitTime, CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);

            try {
                var xmlDocument = await _fileReader.Read(_path);
                var clientElems = xmlDocument.Root?.HasElements ?? false
                    ? xmlDocument.Root.Element(Constants.XmlNames.Elem)?.Elements() ?? Enumerable.Empty<XElement>()
                    : Enumerable.Empty<XElement>();

                var clients = new List<ClientDataRecord>();

                foreach (var clientElem in clientElems) {
                    var clientDataRecord = _clientDataRecordSerializer.Deserialize(clientElem);
                    clients.Add(clientDataRecord);
                }
                
                return clients;
            }
            finally {
                _semaphore.Release();
            }
        }

        public void Dispose() {
            _semaphore?.Dispose();
        }
    }
}