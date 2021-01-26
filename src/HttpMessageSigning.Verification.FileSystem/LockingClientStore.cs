using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification.FileSystem {
    internal class LockingClientStore : IClientStore {
        private static readonly TimeSpan MaxLockWaitTime = TimeSpan.FromSeconds(1);

        private readonly IClientStore _decorated;
        private readonly SemaphoreSlim _semaphore;

        public LockingClientStore(IClientStore decorated, ISemaphoreFactory semaphoreFactory) {
            if (semaphoreFactory == null) throw new ArgumentNullException(nameof(semaphoreFactory));
            _decorated = decorated ?? throw new ArgumentNullException(nameof(decorated));
            _semaphore = semaphoreFactory.CreateLock();
        }

        public void Dispose() {
            _semaphore?.Dispose();
            _decorated?.Dispose();
        }

        public async Task Register(Client client) {
            await _semaphore.WaitAsync(MaxLockWaitTime, CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);

            try {
                await _decorated.Register(client);
            }
            finally {
                _semaphore.Release();
            }
        }

        public async Task<Client> Get(KeyId clientId) {
            await _semaphore.WaitAsync(MaxLockWaitTime, CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);

            try {
                return await _decorated.Get(clientId);
            }
            finally {
                _semaphore.Release();
            }
        }
    }
}