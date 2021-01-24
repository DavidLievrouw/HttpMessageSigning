using System;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    internal class BackgroundTaskStarter : IBackgroundTaskStarter {
        private readonly IDelayer _delayer;
        
        public BackgroundTaskStarter(IDelayer delayer) {
            _delayer = delayer ?? throw new ArgumentNullException(nameof(delayer));
        }

        public void Start(Func<Task> task) {
            if (task == null) throw new ArgumentNullException(nameof(task));

            Start(task, TimeSpan.Zero);
        }

        public void Start(Func<Task> task, TimeSpan delay) {
            if (task == null) throw new ArgumentNullException(nameof(task));
            
#pragma warning disable 4014
            // Fire-and-forget
            Task.Factory.StartNew(async () => {
                await _delayer.Delay(delay).ConfigureAwait(continueOnCapturedContext: false);
                await task().ConfigureAwait(continueOnCapturedContext: false);
            });
#pragma warning restore 4014
        }
    }
}