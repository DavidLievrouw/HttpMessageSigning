using System;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.Utils;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    public class ImmediateBackgroundTaskStarter : IBackgroundTaskStarter {
        public void Start(Func<Task> task) {
            task.Invoke().GetAwaiter().GetResult();
            InvocationCount++;
        }

        public void Start(Func<Task> task, TimeSpan delay) {
            Start(task);
        }
        
        public int InvocationCount { get; private set; }
    }
}