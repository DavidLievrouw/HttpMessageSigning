using System;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using Dapper;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    internal class SqlServerNonceStore : ISqlServerNonceStore {
        private const string TableNameToken = "{TableName}";
        private readonly string _connectionString;
        private readonly IExpiredNoncesCleaner _expiredNoncesCleaner;
        private readonly Lazy<string> _getSql;

        private readonly Lazy<string> _mergeSql;

        public SqlServerNonceStore(string connectionString, string tableName, IExpiredNoncesCleaner expiredNoncesCleaner) {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentException("Value cannot be null or empty.", nameof(tableName));
            if (string.IsNullOrEmpty(connectionString)) throw new ArgumentException("Value cannot be null or empty.", nameof(connectionString));
            
            _connectionString = connectionString;
            _expiredNoncesCleaner = expiredNoncesCleaner ?? throw new ArgumentNullException(nameof(expiredNoncesCleaner));
            
            _mergeSql = new Lazy<string>(() => {
                var thisNamespace = typeof(SqlServerNonceStore).Namespace;
                using (var stream = typeof(SqlServerNonceStore).Assembly.GetManifestResourceStream($"{thisNamespace}.Scripts.MergeNonce.sql")) {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    using (var streamReader = new StreamReader(stream)) {
                        var template = streamReader.ReadToEnd();
                        return template.Replace(TableNameToken, tableName);
                    }
                }
            });
            _getSql = new Lazy<string>(() => {
                var thisNamespace = typeof(SqlServerNonceStore).Namespace;
                using (var stream = typeof(SqlServerNonceStore).Assembly.GetManifestResourceStream($"{thisNamespace}.Scripts.GetNonce.sql")) {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    using (var streamReader = new StreamReader(stream)) {
                        var template = streamReader.ReadToEnd();
                        return template.Replace(TableNameToken, tableName);
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
            using (var connection = new SqlConnection(_connectionString)) {
                var record = new NonceDataRecord {
                    ClientId = nonce.ClientId,
                    Value = nonce.Value,
                    Expiration = nonce.Expiration
                };

                // Do not return Task here. Await it.
                // Otherwise, the SqlConnection is disposed too early.
                // See https://stackoverflow.com/questions/39279094/why-is-taskcanceledexception-thrown-when-using-dapper-queryasynct-without-asyn.
                var recordsAffected = await connection.ExecuteAsync(sql, new[] {record});

                if (recordsAffected < 1) {
                    throw new HttpMessageSigningException($"The specified nonce for client '{nonce.ClientId}' was not stored in the database.");
                }
            }

            await _expiredNoncesCleaner.CleanUpNonces();
        }

        public async Task<Nonce> Get(KeyId clientId, string nonceValue) {
            if (clientId == KeyId.Empty) throw new ArgumentException("Value cannot be null or empty.", nameof(clientId));
            if (string.IsNullOrEmpty(nonceValue)) throw new ArgumentException("Value cannot be null or empty.", nameof(nonceValue));

            var sql = _getSql.Value;
            using (var connection = new SqlConnection(_connectionString)) {
                var nonce = await connection.QuerySingleOrDefaultAsync<NonceDataRecord>(sql, new {ClientId = clientId.Value, Value = nonceValue});
                return nonce == null
                    ? null
                    : new Nonce(new KeyId(nonce.ClientId), nonce.Value, nonce.Expiration);
            }
        }
    }
}