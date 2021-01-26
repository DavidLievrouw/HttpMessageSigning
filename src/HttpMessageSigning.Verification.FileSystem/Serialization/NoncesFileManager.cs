using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Dalion.HttpMessageSigning.Verification.FileSystem.Serialization {
    internal class NoncesFileManager : IFileManager<NonceDataRecord>, IDisposable {
        private readonly IFileReader _fileReader;
        private readonly IFileWriter _fileWriter;
        private readonly string _path;
        private readonly INonceDataRecordSerializer _nonceDataRecordSerializer;
        private readonly SemaphoreSlim _semaphore;
        
        private static readonly TimeSpan MaxLockWaitTime = TimeSpan.FromSeconds(1);

        public NoncesFileManager(IFileReader fileReader, IFileWriter fileWriter, string path, ISemaphoreFactory semaphoreFactory, INonceDataRecordSerializer nonceDataRecordSerializer) {
            if (semaphoreFactory == null) throw new ArgumentNullException(nameof(semaphoreFactory));
            if (string.IsNullOrEmpty(path)) throw new ArgumentException("Value cannot be null or empty.", nameof(path));
            _fileReader = fileReader ?? throw new ArgumentNullException(nameof(fileReader));
            _fileWriter = fileWriter ?? throw new ArgumentNullException(nameof(fileWriter));
            _path = path;
            _nonceDataRecordSerializer = nonceDataRecordSerializer ?? throw new ArgumentNullException(nameof(nonceDataRecordSerializer));
            _semaphore = semaphoreFactory.CreateLock();
        }

        public async Task Write(IEnumerable<NonceDataRecord> nonces) {
            if (nonces == null) throw new ArgumentNullException(nameof(nonces));
            
            await _semaphore.WaitAsync(MaxLockWaitTime, CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);

            try {
                var xmlRoot = new XElement(XName.Get(Constants.XmlNames.Root, Constants.XmlNames.Ns));
                xmlRoot.SetAttributeValue("xmlns", Constants.XmlNames.Ns);
                var noncesElem = new XElement(Constants.XmlNames.NoncesElem);
                foreach (var nonce in nonces) {
                    var serialized = _nonceDataRecordSerializer.Serialize(nonce);
                    noncesElem.Add(serialized);
                }
                xmlRoot.Add(noncesElem);
                var xmlDocument = new XDocument(xmlRoot);
                await _fileWriter.Write(_path, xmlDocument);
            }
            finally {
                _semaphore.Release();
            }
        }

        public async Task<IEnumerable<NonceDataRecord>> Read() {
            await _semaphore.WaitAsync(MaxLockWaitTime, CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);

            try {
                var xmlDocument = await _fileReader.Read(_path);
                var nonceElems = xmlDocument.Root?.HasElements ?? false
                    ? xmlDocument.Root.Element(Constants.XmlNames.NoncesElem)?.Elements() ?? Enumerable.Empty<XElement>()
                    : Enumerable.Empty<XElement>();

                var nonces = new List<NonceDataRecord>();

                foreach (var nonceElem in nonceElems) {
                    var nonceDataRecord = _nonceDataRecordSerializer.Deserialize(nonceElem);
                    nonces.Add(nonceDataRecord);
                }
                
                return nonces;
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