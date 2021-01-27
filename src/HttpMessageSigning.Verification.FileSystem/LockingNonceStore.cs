using System;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace Dalion.HttpMessageSigning.Verification.FileSystem {
    internal class LockingNonceStore : INonceStore {
        private readonly INonceStore _decorated;
        private readonly AsyncReaderWriterLock _lock;

        public LockingNonceStore(INonceStore decorated, ILockFactory lockFactory) {
            if (lockFactory == null) throw new ArgumentNullException(nameof(lockFactory));
            _decorated = decorated ?? throw new ArgumentNullException(nameof(decorated));
            _lock = lockFactory.CreateLock();
        }

        public void Dispose() {
            _decorated?.Dispose();
        }

        public async Task Register(Nonce nonce) {
            using (await _lock.WriterLockAsync()) {
                await _decorated.Register(nonce);
            }
        }

        public async Task<Nonce> Get(KeyId clientId, string nonceValue) {
            using (await _lock.ReaderLockAsync()) {
                return await _decorated.Get(clientId, nonceValue);
            }
        }
    }
}