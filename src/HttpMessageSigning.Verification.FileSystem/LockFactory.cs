using Nito.AsyncEx;

namespace Dalion.HttpMessageSigning.Verification.FileSystem {
    internal class LockFactory : ILockFactory {
        public AsyncReaderWriterLock CreateLock() {
            return new AsyncReaderWriterLock();
        }
    }
}