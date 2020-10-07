using System.Threading;

namespace Dalion.HttpMessageSigning.Verification.MongoDb.ClientStoreMigrations {
    internal interface ISemaphoreFactory {
        SemaphoreSlim CreateLock();
    }
}