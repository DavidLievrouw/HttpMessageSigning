using System.Threading;

namespace Dalion.HttpMessageSigning.Verification.MongoDb.Migrations {
    internal interface ISemaphoreFactory {
        SemaphoreSlim CreateLock();
    }
}