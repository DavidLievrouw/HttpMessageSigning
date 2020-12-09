using System;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    internal class SqlServerNonceStore : ISqlServerNonceStore {

        public SqlServerNonceStore(string tableName) {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentException("Value cannot be null or empty.", nameof(tableName));
        }

        public void Dispose() {
            // Noop
        }

        public async Task Register(Nonce nonce) {
            if (nonce == null) throw new ArgumentNullException(nameof(nonce));

            var nonceId = CreateId(nonce.ClientId, nonce.Value);
            var record = new NonceDataRecord {
                Id = nonceId,
                ClientId = nonce.ClientId,
                Value = nonce.Value,
                Expiration = nonce.Expiration.UtcDateTime
            };

            throw new NotImplementedException();
        }

        public async Task<Nonce> Get(KeyId clientId, string nonceValue) {
            if (clientId == KeyId.Empty) throw new ArgumentException("Value cannot be null or empty.", nameof(clientId));
            if (string.IsNullOrEmpty(nonceValue)) throw new ArgumentException("Value cannot be null or empty.", nameof(nonceValue));

            throw new NotImplementedException();
            /*var collection = _lazyCollection.Value;
            
            var nonceId = CreateId(clientId, nonceValue);
            var findResult = await collection.FindAsync(r => r.Id == nonceId).ConfigureAwait(false);
            var matches = await findResult.ToListAsync().ConfigureAwait(false);
            if (!matches.Any()) return null;

            var match = matches.OrderByDescending(_ => _.Expiration).First();

            return new Nonce(new KeyId(match.ClientId), match.Value, match.Expiration);*/
        }

        private static string CreateId(KeyId clientId, string nonceValue) {
            return $"{clientId}_{nonceValue}";
        }
    }
}