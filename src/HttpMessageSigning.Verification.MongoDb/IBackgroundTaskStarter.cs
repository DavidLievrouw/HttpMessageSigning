using System;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    internal interface IBackgroundTaskStarter {
        void Start(Func<Task> task);
        void Start(Func<Task> task, TimeSpan delay);
    }
}