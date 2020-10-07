using System.Threading;

namespace Dalion.HttpMessageSigning.Verification.MongoDb.Migrations {
    internal class SemaphoreFactory : ISemaphoreFactory {
        public SemaphoreSlim CreateLock() {
            return new SemaphoreSlim(1, 1);
        }
    }
}