using Nito.AsyncEx;

namespace Dalion.HttpMessageSigning.Verification.FileSystem {
    internal interface ILockFactory {
        AsyncReaderWriterLock CreateLock();
    }
}