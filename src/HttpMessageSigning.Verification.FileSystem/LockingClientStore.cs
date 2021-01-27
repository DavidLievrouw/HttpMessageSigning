using System;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace Dalion.HttpMessageSigning.Verification.FileSystem {
    internal class LockingClientStore : IClientStore {
        private readonly IClientStore _decorated;
        private readonly AsyncReaderWriterLock _lock;

        public LockingClientStore(IClientStore decorated, ILockFactory lockFactory) {
            if (lockFactory == null) throw new ArgumentNullException(nameof(lockFactory));
            _decorated = decorated ?? throw new ArgumentNullException(nameof(decorated));
            _lock = lockFactory.CreateLock();
        }

        public void Dispose() {
            _decorated?.Dispose();
        }

        public async Task Register(Client client) {
            using (await _lock.WriterLockAsync()) {
                await _decorated.Register(client);
            }
        }

        public async Task<Client> Get(KeyId clientId) {
            using (await _lock.ReaderLockAsync()) {
                return await _decorated.Get(clientId);
            }
        }
    }
}