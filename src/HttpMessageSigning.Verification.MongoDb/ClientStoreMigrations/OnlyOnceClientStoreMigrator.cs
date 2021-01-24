using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification.MongoDb.ClientStoreMigrations {
    internal class OnlyOnceClientStoreMigrator : IClientStoreMigrator {
        private static readonly TimeSpan MaxLockWaitTime = TimeSpan.FromSeconds(1);

        private readonly IClientStoreMigrator _decorated;
        private readonly ISemaphoreFactory _semaphoreFactory;
        private readonly Lazy<Task<int?>> _lazyBaseline;
        private readonly SemaphoreSlim _semaphore;
        private int? _runResult;

        public OnlyOnceClientStoreMigrator(IClientStoreMigrator decorated, IClientStoreBaseliner baseliner, ISemaphoreFactory semaphoreFactory) {
            if (baseliner == null) throw new ArgumentNullException(nameof(baseliner));
            _decorated = decorated ?? throw new ArgumentNullException(nameof(decorated));
            _semaphoreFactory = semaphoreFactory ?? throw new ArgumentNullException(nameof(semaphoreFactory));
            _semaphore = _semaphoreFactory.CreateLock();
            _lazyBaseline = new Lazy<Task<int?>>(baseliner.GetBaseline, LazyThreadSafetyMode.PublicationOnly);
            _runResult = null;
        }

        public async Task<int> Migrate() {
            if (_runResult.HasValue) return _runResult.Value; // Already run once, don't do it again
            
            if (_semaphore.CurrentCount < 1) return await _lazyBaseline.Value.ConfigureAwait(continueOnCapturedContext: false) ?? 0; // Currently running, don't start it again

            await _semaphore.WaitAsync(MaxLockWaitTime, CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);

            try {
                _runResult = await _decorated.Migrate().ConfigureAwait(continueOnCapturedContext: false);
                return _runResult.Value;
            }
            finally {
                _semaphore.Release();
            }
        }
    }
}