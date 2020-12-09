using System.Threading;

namespace Dalion.HttpMessageSigning.Verification.SqlServer.ClientStoreMigrations {
    internal interface ISemaphoreFactory {
        SemaphoreSlim CreateLock();
    }
}