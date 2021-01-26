using System.Threading;

namespace Dalion.HttpMessageSigning.Verification.FileSystem {
    internal interface ISemaphoreFactory {
        SemaphoreSlim CreateLock();
    }
}