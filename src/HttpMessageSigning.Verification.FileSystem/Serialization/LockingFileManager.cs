using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification.FileSystem.Serialization {
    internal class LockingFileManager<TData> : IFileManager<TData>, IDisposable {
        private static readonly TimeSpan MaxLockWaitTime = TimeSpan.FromSeconds(1);
        
        private readonly IFileManager<TData> _decorated;
        private readonly SemaphoreSlim _semaphore;

        public LockingFileManager(IFileManager<TData> decorated, ISemaphoreFactory semaphoreFactory) {
            if (semaphoreFactory == null) throw new ArgumentNullException(nameof(semaphoreFactory));
            _decorated = decorated ?? throw new ArgumentNullException(nameof(decorated));
            _semaphore = semaphoreFactory.CreateLock();
        }

        public void Dispose() {
            _semaphore?.Dispose();
        }

        public async Task Write(IEnumerable<TData> data) {
            await _semaphore.WaitAsync(MaxLockWaitTime, CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);

            try {
                await _decorated.Write(data);
            }
            finally {
                _semaphore.Release();
            }
        }

        public async Task<IEnumerable<TData>> Read() {
            await _semaphore.WaitAsync(MaxLockWaitTime, CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);

            try {
                return await _decorated.Read();
            }
            finally {
                _semaphore.Release();
            }
        }
    }
}