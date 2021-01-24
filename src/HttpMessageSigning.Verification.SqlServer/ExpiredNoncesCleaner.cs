using System;
using System.Data.SqlClient;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Dapper;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    internal class ExpiredNoncesCleaner : IExpiredNoncesCleaner {
        private const string TableNameToken = "{TableName}";
        private readonly SqlServerNonceStoreSettings _settings;
        private readonly IBackgroundTaskStarter _backgroundTaskStarter;
        private readonly ISystemClock _systemClock;
        private readonly Lazy<string> _deleteExpiredSql;
        private DateTimeOffset _lastCleanUp;
        private readonly SemaphoreSlim _semaphore;

        public ExpiredNoncesCleaner(SqlServerNonceStoreSettings settings, IBackgroundTaskStarter backgroundTaskStarter, ISystemClock systemClock) {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _backgroundTaskStarter = backgroundTaskStarter ?? throw new ArgumentNullException(nameof(backgroundTaskStarter));
            _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));

            _deleteExpiredSql = new Lazy<string>(() => {
                var thisNamespace = typeof(SqlServerNonceStore).Namespace;
                using (var stream = typeof(SqlServerNonceStore).Assembly.GetManifestResourceStream($"{thisNamespace}.Scripts.DeleteExpiredNonces.sql")) {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    using (var streamReader = new StreamReader(stream)) {
                        var template = streamReader.ReadToEnd();
                        return template.Replace(TableNameToken, settings.NonceTableName);
                    }
                }
            });

            _lastCleanUp = DateTimeOffset.MinValue;
            _semaphore = new SemaphoreSlim(1, 1);
        }

        public Task CleanUpNonces() {
            if (_lastCleanUp.Add(_settings.ExpiredNoncesCleanUpInterval) > _systemClock.UtcNow) return Task.CompletedTask;
            
            _backgroundTaskStarter.Start(RunCleanUp, TimeSpan.FromSeconds(5));
            return Task.CompletedTask;
        }

        private async Task RunCleanUp() {
            await _semaphore.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
            
            try {
                var sql = _deleteExpiredSql.Value;
                using (var connection = new SqlConnection(_settings.ConnectionString)) {
                    await connection.ExecuteAsync(sql, new { Now = _systemClock.UtcNow }).ConfigureAwait(continueOnCapturedContext: false);
                }

                _lastCleanUp = _systemClock.UtcNow;
            }
            finally {
                _semaphore.Release();
            }
        }

        public void Dispose() {
            _semaphore?.Dispose();
        }
    }
}