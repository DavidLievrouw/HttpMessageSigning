using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    internal class SqlServerClientStore : ISqlServerClientStore {
        private const string ClientsTableNameToken = "{ClientsTableName}";
        private const string ClientClaimsTableNameToken = "{ClientClaimsTableName}";

        private readonly Lazy<string> _getSql;
        private readonly Lazy<string> _deleteClientClaimsSql;
        private readonly Lazy<string> _insertClientClaimSql;
        private readonly Lazy<string> _mergeClientSql;
        private readonly SqlServerClientStoreSettings _settings;
        private readonly ISignatureAlgorithmConverter _signatureAlgorithmConverter;

        public SqlServerClientStore(SqlServerClientStoreSettings settings, ISignatureAlgorithmConverter signatureAlgorithmConverter) {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _signatureAlgorithmConverter = signatureAlgorithmConverter ?? throw new ArgumentNullException(nameof(signatureAlgorithmConverter));

            _getSql = new Lazy<string>(() => {
                var thisNamespace = typeof(SqlServerNonceStore).Namespace;
                using (var stream = typeof(SqlServerNonceStore).Assembly.GetManifestResourceStream($"{thisNamespace}.Scripts.GetClient.sql")) {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    using (var streamReader = new StreamReader(stream)) {
                        var template = streamReader.ReadToEnd();
                        return template
                            .Replace(ClientsTableNameToken, _settings.ClientsTableName)
                            .Replace(ClientClaimsTableNameToken, _settings.ClientClaimsTableName);
                    }
                }
            });
            _mergeClientSql = new Lazy<string>(() => {
                var thisNamespace = typeof(SqlServerNonceStore).Namespace;
                using (var stream = typeof(SqlServerNonceStore).Assembly.GetManifestResourceStream($"{thisNamespace}.Scripts.MergeClient.sql")) {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    using (var streamReader = new StreamReader(stream)) {
                        var template = streamReader.ReadToEnd();
                        return template
                            .Replace(ClientsTableNameToken, _settings.ClientsTableName);
                    }
                }
            });
            _deleteClientClaimsSql = new Lazy<string>(() => {
                var thisNamespace = typeof(SqlServerNonceStore).Namespace;
                using (var stream = typeof(SqlServerNonceStore).Assembly.GetManifestResourceStream($"{thisNamespace}.Scripts.DeleteClientClaims.sql")) {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    using (var streamReader = new StreamReader(stream)) {
                        var template = streamReader.ReadToEnd();
                        return template
                            .Replace(ClientClaimsTableNameToken, _settings.ClientClaimsTableName);
                    }
                }
            });
            _insertClientClaimSql = new Lazy<string>(() => {
                var thisNamespace = typeof(SqlServerNonceStore).Namespace;
                using (var stream = typeof(SqlServerNonceStore).Assembly.GetManifestResourceStream($"{thisNamespace}.Scripts.InsertClientClaim.sql")) {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    using (var streamReader = new StreamReader(stream)) {
                        var template = streamReader.ReadToEnd();
                        return template
                            .Replace(ClientClaimsTableNameToken, _settings.ClientClaimsTableName);
                    }
                }
            });
        }

        public void Dispose() {
            // Noop
        }

        public async Task Register(Client client) {
            if (client == null) throw new ArgumentNullException(nameof(client));

            var clientRecord = new ClientDataRecord {
                Id = client.Id,
                Name = client.Name,
                NonceLifetime = client.NonceLifetime.TotalSeconds,
                ClockSkew = client.ClockSkew.TotalSeconds,
                RequestTargetEscaping = client.RequestTargetEscaping.ToString(),
                Claims = client.Claims?.Select(c => ClaimDataRecord.FromClaim(client.Id, c))?.ToList() ?? new List<ClaimDataRecord>(),
                V = ClientDataRecord.GetV()
            };
            _signatureAlgorithmConverter.SetSignatureAlgorithm(clientRecord, client.SignatureAlgorithm, _settings.SharedSecretEncryptionKey);

            using (var connection = new SqlConnection(_settings.ConnectionString)) {
                await connection.OpenAsync();
                try {
                    using (var transaction = connection.BeginTransaction()) {
                        await connection.ExecuteAsync(_mergeClientSql.Value, clientRecord, transaction).ConfigureAwait(continueOnCapturedContext: false);
                        await connection.ExecuteAsync(_deleteClientClaimsSql.Value, new {ClientId = client.Id.Value}, transaction).ConfigureAwait(continueOnCapturedContext: false);
                        if (clientRecord.Claims.Count > 0) {
                            foreach (var claim in clientRecord.Claims) {
                                await connection.ExecuteAsync(_insertClientClaimSql.Value, claim, transaction).ConfigureAwait(continueOnCapturedContext: false);
                            }
                        }
                        transaction.Commit();
                    }
                }
                finally {
                    connection.Close();
                }
            }
        }

        public async Task<Client> Get(KeyId clientId) {
            if (clientId == KeyId.Empty) throw new ArgumentException("Value cannot be null or empty.", nameof(clientId));

            IList<ClientDataRecord> matches; 
            using (var connection = new SqlConnection(_settings.ConnectionString)) {
                var results = await connection.QueryAsync<ClientDataRecord, ClaimDataRecord, ClientDataRecord>(
                    _getSql.Value,
                    (client, claim) => {
                        // ReSharper disable once ConvertToNullCoalescingCompoundAssignment
                        client.Claims = client.Claims ?? new List<ClaimDataRecord>();
                        client.Claims.Add(claim);
                        return client;
                    },
                    new {ClientId = clientId.Value},
                    splitOn: nameof(ClaimDataRecord.ClientId))
                    .ConfigureAwait(continueOnCapturedContext: false);
                matches = results
                    .GroupBy(clientDataRecord => clientDataRecord.Id)
                    .Select(_ => {
                        var client = _.First();
                        client.Claims = _.SelectMany(c => c.Claims).ToList();
                        return client;
                    })
                    .ToList();
            }

            if (!matches.Any()) return null;
            
            var match = matches.Single();
            
            var nonceLifetime = !match.NonceLifetime.HasValue || match.NonceLifetime.Value <= 0.0
                ? ClientOptions.Default.NonceLifetime
                : TimeSpan.FromSeconds(match.NonceLifetime.Value);
            
            var clockSkew = !match.ClockSkew.HasValue || match.ClockSkew.Value <= 0.0
                ? ClientOptions.Default.ClockSkew
                : TimeSpan.FromSeconds(match.ClockSkew.Value);

            var requestTargetEscaping = RequestTargetEscaping.RFC3986;
            if (!string.IsNullOrEmpty(match.RequestTargetEscaping)) {
                if (Enum.TryParse<RequestTargetEscaping>(match.RequestTargetEscaping, ignoreCase: true, out var parsed)) {
                    requestTargetEscaping = parsed;
                }
            }

            var signatureAlgorithm = _signatureAlgorithmConverter.ToSignatureAlgorithm(match, _settings.SharedSecretEncryptionKey);
            
            return new Client(
                match.Id,
                match.Name,
                signatureAlgorithm,
                nonceLifetime,
                clockSkew,
                requestTargetEscaping,
                match.Claims?.Select(c => c.ToClaim())?.ToArray());
        }
    }
}