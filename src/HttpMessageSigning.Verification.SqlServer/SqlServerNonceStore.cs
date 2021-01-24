using System;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using Dapper;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    internal class SqlServerNonceStore : ISqlServerNonceStore {
        private const string TableNameToken = "{TableName}";
        private readonly SqlServerNonceStoreSettings _settings;
        private readonly IExpiredNoncesCleaner _expiredNoncesCleaner;
        private readonly Lazy<string> _getSql;

        private readonly Lazy<string> _mergeSql;

        public SqlServerNonceStore(SqlServerNonceStoreSettings settings, IExpiredNoncesCleaner expiredNoncesCleaner) {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _expiredNoncesCleaner = expiredNoncesCleaner ?? throw new ArgumentNullException(nameof(expiredNoncesCleaner));
            
            _mergeSql = new Lazy<string>(() => {
                var thisNamespace = typeof(SqlServerNonceStore).Namespace;
                using (var stream = typeof(SqlServerNonceStore).Assembly.GetManifestResourceStream($"{thisNamespace}.Scripts.MergeNonce.sql")) {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    using (var streamReader = new StreamReader(stream)) {
                        var template = streamReader.ReadToEnd();
                        return template.Replace(TableNameToken, _settings.NonceTableName);
                    }
                }
            });
            _getSql = new Lazy<string>(() => {
                var thisNamespace = typeof(SqlServerNonceStore).Namespace;
                using (var stream = typeof(SqlServerNonceStore).Assembly.GetManifestResourceStream($"{thisNamespace}.Scripts.GetNonce.sql")) {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    using (var streamReader = new StreamReader(stream)) {
                        var template = streamReader.ReadToEnd();
                        return template.Replace(TableNameToken, _settings.NonceTableName);
                    }
                }
            });
        }

        public void Dispose() {
            // Noop
        }

        public async Task Register(Nonce nonce) {
            if (nonce == null) throw new ArgumentNullException(nameof(nonce));

            var sql = _mergeSql.Value;
            using (var connection = new SqlConnection(_settings.ConnectionString)) {
                var record = new NonceDataRecord {
                    ClientId = nonce.ClientId,
                    Value = nonce.Value,
                    Expiration = nonce.Expiration
                };
                record.V = record.GetV();

                // Do not return Task here. Await it.
                // Otherwise, the SqlConnection is disposed too early.
                // See https://stackoverflow.com/questions/39279094/why-is-taskcanceledexception-thrown-when-using-dapper-queryasynct-without-asyn.
                var recordsAffected = await connection.ExecuteAsync(sql, new[] {record}).ConfigureAwait(continueOnCapturedContext: false);

                if (recordsAffected < 1) {
                    throw new HttpMessageSigningException($"The specified nonce for client '{nonce.ClientId}' was not stored in the database.");
                }
            }

            await _expiredNoncesCleaner.CleanUpNonces().ConfigureAwait(continueOnCapturedContext: false);
        }

        public async Task<Nonce> Get(KeyId clientId, string nonceValue) {
            if (clientId == KeyId.Empty) throw new ArgumentException("Value cannot be null or empty.", nameof(clientId));
            if (string.IsNullOrEmpty(nonceValue)) throw new ArgumentException("Value cannot be null or empty.", nameof(nonceValue));

            var sql = _getSql.Value;
            using (var connection = new SqlConnection(_settings.ConnectionString)) {
                var nonce = await connection
                    .QuerySingleOrDefaultAsync<NonceDataRecord>(sql, new {ClientId = clientId.Value, Value = nonceValue})
                    .ConfigureAwait(continueOnCapturedContext: false);
                return nonce == null
                    ? null
                    : new Nonce(new KeyId(nonce.ClientId), nonce.Value, nonce.Expiration);
            }
        }
    }
}