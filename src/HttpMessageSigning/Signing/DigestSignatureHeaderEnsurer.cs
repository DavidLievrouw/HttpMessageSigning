using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Signing {
    internal class DigestSignatureHeaderEnsurer : ISignatureHeaderEnsurer {
        public Task EnsureHeader(HttpRequestMessage request, SigningSettings signingSettings, DateTimeOffset timeOfSigning) {
            /*
                    if (_request.Method == HttpMethod.Get) return string.Empty;
            
            if (_request.Content == null) return "\n" + new Header(HeaderName.PredefinedHeaderNames.Digest, string.Empty);
            
            var bodyText = _request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            using (var hashAlgorithm = _hashAlgorithmFactory.Create(_hashAlgorithm)) {
                var payloadBytes = hashAlgorithm.ComputeHash(bodyText);
                var digest = _base64Converter.ToBase64(payloadBytes);
                return "\n" + new Header(HeaderName.PredefinedHeaderNames.Digest, $"{hashAlgorithm.Name}={digest}");
            }
            }*/
            throw new System.NotImplementedException();
        }
    }
}