using System;
using System.Linq;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.Verification.FileSystem.Serialization;

namespace Dalion.HttpMessageSigning.Verification.FileSystem {
    internal class FileSystemClientStore : IClientStore {
        private readonly IFileManager<ClientDataRecord> _fileManager;
        private readonly ISignatureAlgorithmDataRecordConverter _signatureAlgorithmDataRecordConverter;
        private readonly SharedSecretEncryptionKey _encryptionKey;

        public FileSystemClientStore(
            IFileManager<ClientDataRecord> fileManager, 
            ISignatureAlgorithmDataRecordConverter signatureAlgorithmDataRecordConverter, 
            SharedSecretEncryptionKey encryptionKey) {
            _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
            _signatureAlgorithmDataRecordConverter = signatureAlgorithmDataRecordConverter ?? throw new ArgumentNullException(nameof(signatureAlgorithmDataRecordConverter));
            _encryptionKey = encryptionKey;
        }

        public void Dispose() {
            // Noop
        }

        public async Task Register(Client client) {
            if (client == null) throw new ArgumentNullException(nameof(client));

            var record = new ClientDataRecord {
                Id = client.Id,
                Name = client.Name,
                NonceLifetime = client.NonceLifetime.TotalSeconds,
                ClockSkew = client.ClockSkew.TotalSeconds,
                Claims = client.Claims?.Select(ClaimDataRecord.FromClaim)?.ToArray(),
                SigAlg = _signatureAlgorithmDataRecordConverter.FromSignatureAlgorithm(client.SignatureAlgorithm, _encryptionKey),
                Escaping = client.RequestTargetEscaping.ToString(),
                V = ClientDataRecord.GetV()
            };

            var currentClients = await _fileManager.Read();
            var newClients = currentClients.Where(c => c.Id != client.Id).Concat(new[] {record}).ToList();

            await _fileManager.Write(newClients);
        }

        public async Task<Client> Get(KeyId clientId) {
            if (clientId == KeyId.Empty) throw new ArgumentException("Value cannot be null or empty.", nameof(clientId));
            
            var currentClients = await _fileManager.Read();
            var matches = currentClients.Where(c => c.Id == clientId).ToList();
            if (!matches.Any()) return null;
            var match = matches.Single();

            var nonceLifetime = !match.NonceLifetime.HasValue || match.NonceLifetime.Value <= 0.0
                ? ClientOptions.Default.NonceLifetime
                : TimeSpan.FromSeconds(match.NonceLifetime.Value);
            
            var clockSkew = !match.ClockSkew.HasValue || match.ClockSkew.Value <= 0.0
                ? ClientOptions.Default.ClockSkew
                : TimeSpan.FromSeconds(match.ClockSkew.Value);

            var requestTargetEscaping = RequestTargetEscaping.RFC3986;
            if (!string.IsNullOrEmpty(match.Escaping)) {
                if (Enum.TryParse<RequestTargetEscaping>(match.Escaping, ignoreCase: true, out var parsed)) {
                    requestTargetEscaping = parsed;
                }
            }

            var signatureAlgorithm = _signatureAlgorithmDataRecordConverter.ToSignatureAlgorithm(match.SigAlg, _encryptionKey, match.V);
            
            return new Client(
                (KeyId)match.Id,
                match.Name,
                signatureAlgorithm,
                nonceLifetime,
                clockSkew,
                requestTargetEscaping,
                match.Claims?.Select(c => c.ToClaim())?.ToArray());
        }
    }
}