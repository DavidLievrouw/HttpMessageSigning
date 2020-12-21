using System;
using System.Linq;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    internal class SqlServerClientStore : ISqlServerClientStore {
        private readonly SqlServerClientStoreSettings _settings;

        public SqlServerClientStore(SqlServerClientStoreSettings settings) {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public void Dispose() {
            // Noop
        }

        public async Task Register(Client client) {
            if (client == null) throw new ArgumentNullException(nameof(client));
            
            if (IsProhibitedId(client.Id)) throw new ArgumentException($"The id value of the specified {nameof(Client)} is prohibited ({client.Id}).", nameof(client));
            
            var record = new ClientDataRecord {
                Id = client.Id,
                Name = client.Name,
                NonceLifetime = client.NonceLifetime.TotalSeconds,
                ClockSkew = client.ClockSkew.TotalSeconds,
                Claims = client.Claims?.Select(ClaimDataRecord.FromClaim)?.ToArray(),
                SignatureAlgorithm = SignatureAlgorithmDataRecord.FromSignatureAlgorithm(client.SignatureAlgorithm, _settings.SharedSecretEncryptionKey),
                RequestTargetEscaping = client.RequestTargetEscaping.ToString()
            };
            record.V = record.GetV();

            throw new NotImplementedException();
        }

        public async Task<Client> Get(KeyId clientId) {
            if (clientId == KeyId.Empty) throw new ArgumentException("Value cannot be null or empty.", nameof(clientId));
            
            if (IsProhibitedId(clientId)) return null;
            
            throw new NotImplementedException();
            /*var collection = _lazyCollection.Value;

            var findResult = await collection.FindAsync(r => r.Id == clientId).ConfigureAwait(continueOnCapturedContext: false);
            var matches = await findResult.ToListAsync().ConfigureAwait(continueOnCapturedContext: false);
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
            
            return new Client(
                match.Id,
                match.Name,
                match.SignatureAlgorithm.ToSignatureAlgorithm(_encryptionKey, match.V),
                nonceLifetime,
                clockSkew,
                requestTargetEscaping,
                match.Claims?.Select(c => c.ToClaim())?.ToArray());*/
        }
        
        private static bool IsProhibitedId(KeyId id) {
            return ProhibitedIds.Contains(id);
        }

        private static readonly KeyId[] ProhibitedIds = {
            KeyId.Empty,
            "_version"
        };
    }
}