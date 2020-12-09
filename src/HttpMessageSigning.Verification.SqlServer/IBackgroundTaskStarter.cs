using System;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    internal interface IBackgroundTaskStarter {
        void Start(Func<Task> task);
        void Start(Func<Task> task, TimeSpan delay);
    }
}