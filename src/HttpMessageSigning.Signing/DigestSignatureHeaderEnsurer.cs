using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Signing {
    internal class DigestSignatureHeaderEnsurer : ISignatureHeaderEnsurer {
        private readonly IBase64Converter _base64Converter;

        public DigestSignatureHeaderEnsurer(IBase64Converter base64Converter) {
            _base64Converter = base64Converter ?? throw new ArgumentNullException(nameof(base64Converter));
        }

        public async Task EnsureHeader(HttpRequestMessage request, SigningSettings signingSettings, DateTimeOffset timeOfSigning) {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (signingSettings == null) throw new ArgumentNullException(nameof(signingSettings));
            
            if (!request.Method.SupportsBody()) return;
            if (string.IsNullOrEmpty(signingSettings.DigestHashAlgorithm.Name)) return;
            if (request.Headers.Contains("Digest")) return;

            if (request.Content == null) {
                request.Headers.Add("Digest", string.Empty);
                return;
            }

            var bodyBytes = await request.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            using (var hashAlgorithm = System.Security.Cryptography.HashAlgorithm.Create(signingSettings.DigestHashAlgorithm.Name)) {
                if (hashAlgorithm == null) throw new NotSupportedException($"The specified hash algorithm ({signingSettings.DigestHashAlgorithm.Name}) for digest is currently not supported.");
                var payloadBytes = hashAlgorithm.ComputeHash(bodyBytes);
                var digest = _base64Converter.ToBase64(payloadBytes);
                var digestAlgorithmName = Constants.DigestHashAlgorithmNames[signingSettings.DigestHashAlgorithm.Name];
                request.Headers.Add("Digest", $"{digestAlgorithmName}={digest}");
            }
        }
    }
}