using System;
using System.Data.SqlClient;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Dapper;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    internal class ExpiredNoncesCleaner : IExpiredNoncesCleaner {
        private const string TableNameToken = "{TableName}";
        private readonly IBackgroundTaskStarter _backgroundTaskStarter;
        private readonly ISystemClock _systemClock;
        private readonly TimeSpan _cleanUpInterval;
        private readonly string _connectionString;
        private readonly Lazy<string> _deleteExpiredSql;
        private DateTimeOffset _lastCleanUp;
        private readonly SemaphoreSlim _semaphore;

        public ExpiredNoncesCleaner(string connectionString, string tableName, IBackgroundTaskStarter backgroundTaskStarter, ISystemClock systemClock, TimeSpan cleanUpInterval) {
            if (string.IsNullOrEmpty(connectionString)) throw new ArgumentException("Value cannot be null or empty.", nameof(connectionString));
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentException("Value cannot be null or empty.", nameof(tableName));
            if (cleanUpInterval <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(cleanUpInterval), cleanUpInterval, "The clean-up interval cannot be zero or negative.");

            _connectionString = connectionString;
            _backgroundTaskStarter = backgroundTaskStarter ?? throw new ArgumentNullException(nameof(backgroundTaskStarter));
            _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
            _cleanUpInterval = cleanUpInterval;

            _deleteExpiredSql = new Lazy<string>(() => {
                var thisNamespace = typeof(SqlServerNonceStore).Namespace;
                using (var stream = typeof(SqlServerNonceStore).Assembly.GetManifestResourceStream($"{thisNamespace}.Scripts.DeleteExpiredNonces.sql")) {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    using (var streamReader = new StreamReader(stream)) {
                        var template = streamReader.ReadToEnd();
                        return template.Replace(TableNameToken, tableName);
                    }
                }
            });

            _lastCleanUp = DateTimeOffset.MinValue;
            _semaphore = new SemaphoreSlim(1, 1);
        }

        public Task CleanUpNonces() {
            if (_lastCleanUp.Add(_cleanUpInterval) > _systemClock.UtcNow) return Task.CompletedTask;
            
            _backgroundTaskStarter.Start(RunCleanUp, TimeSpan.FromSeconds(5));
            return Task.CompletedTask;
        }

        private async Task RunCleanUp() {
            await _semaphore.WaitAsync();
            
            try {
                var sql = _deleteExpiredSql.Value;
                using (var connection = new SqlConnection(_connectionString)) {
                    await connection.ExecuteAsync(sql, new { Now = _systemClock.UtcNow });
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