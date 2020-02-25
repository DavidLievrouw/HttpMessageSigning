using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Signing {
    internal class DigestSignatureHeaderEnsurer : ISignatureHeaderEnsurer {
        private readonly IHashAlgorithmFactory _hashAlgorithmFactory;
        private readonly IBase64Converter _base64Converter;

        public DigestSignatureHeaderEnsurer(IHashAlgorithmFactory hashAlgorithmFactory, IBase64Converter base64Converter) {
            _hashAlgorithmFactory = hashAlgorithmFactory ?? throw new ArgumentNullException(nameof(hashAlgorithmFactory));
            _base64Converter = base64Converter ?? throw new ArgumentNullException(nameof(base64Converter));
        }

        public async Task EnsureHeader(HttpRequestMessage request, SigningSettings signingSettings, DateTimeOffset timeOfSigning) {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (signingSettings == null) throw new ArgumentNullException(nameof(signingSettings));
            
            if (request.Method == HttpMethod.Get) return;
            if (signingSettings.DigestHashAlgorithm == HashAlgorithm.None) return;
            if (request.Headers.Contains("Digest")) return;

            if (request.Content == null) {
                request.Headers.Add("Digest", string.Empty);
                return;
            }

            var bodyText = await request.Content.ReadAsStringAsync();
            using (var hashAlgorithm = _hashAlgorithmFactory.Create(signingSettings.DigestHashAlgorithm)) {
                var payloadBytes = hashAlgorithm.ComputeHash(bodyText);
                var digest = _base64Converter.ToBase64(payloadBytes);
                request.Headers.Add("Digest", $"{hashAlgorithm.Name}={digest}");
            }
        }
    }
}