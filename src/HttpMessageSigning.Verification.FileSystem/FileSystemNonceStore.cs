using System;
using System.Linq;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.Utils;
using Dalion.HttpMessageSigning.Verification.FileSystem.Serialization;

namespace Dalion.HttpMessageSigning.Verification.FileSystem {
    internal class FileSystemNonceStore : INonceStore {
        private readonly IFileManager<NonceDataRecord> _fileManager;
        private readonly ISystemClock _systemClock;

        public FileSystemNonceStore(IFileManager<NonceDataRecord> fileManager, ISystemClock systemClock) {
            _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
            _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
        }

        public void Dispose() {
            // Noop
        }

        public async Task Register(Nonce nonce) {
            if (nonce == null) throw new ArgumentNullException(nameof(nonce));

            var record = new NonceDataRecord {
                ClientId = nonce.ClientId,
                Value = nonce.Value,
                Expiration = nonce.Expiration.UtcDateTime
            };

            var currentNonces = await _fileManager.Read();
            var newNonces = currentNonces
                .Where(n => n.ClientId != nonce.ClientId || n.Value != nonce.Value)
                .Concat(new[] {record})
                .Where(n => n.Expiration > _systemClock.UtcNow);

            await _fileManager.Write(newNonces);
        }

        public async Task<Nonce> Get(KeyId clientId, string nonceValue) {
            if (clientId == KeyId.Empty) throw new ArgumentException("Value cannot be null or empty.", nameof(clientId));
            if (string.IsNullOrEmpty(nonceValue)) throw new ArgumentException("Value cannot be null or empty.", nameof(nonceValue));

            var currentNonces = await _fileManager.Read();

            var matchingNonce = currentNonces
                .Where(n => n.ClientId == clientId && n.Value == nonceValue && n.Expiration >= _systemClock.UtcNow)
                .OrderByDescending(n => n.Expiration)
                .FirstOrDefault();

            return matchingNonce == null
                ? null
                : new Nonce(new KeyId(matchingNonce.ClientId), matchingNonce.Value, matchingNonce.Expiration);
        }
    }
}