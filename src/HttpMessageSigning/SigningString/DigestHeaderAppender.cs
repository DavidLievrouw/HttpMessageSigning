using System;
using System.Net.Http;

namespace Dalion.HttpMessageSigning.SigningString {
    internal class DigestHeaderAppender : IHeaderAppender {
        private readonly HttpRequestMessage _request;
        private readonly HashAlgorithm _hashAlgorithm;
        private readonly IBase64Converter _base64Converter;
        private readonly IHashAlgorithmFactory _hashAlgorithmFactory;

        public DigestHeaderAppender(
            HttpRequestMessage request, 
            HashAlgorithm hashAlgorithm, 
            IBase64Converter base64Converter,
            IHashAlgorithmFactory hashAlgorithmFactory) {
            _request = request ?? throw new ArgumentNullException(nameof(request));
            _hashAlgorithm = hashAlgorithm;
            _base64Converter = base64Converter ?? throw new ArgumentNullException(nameof(base64Converter));
            _hashAlgorithmFactory = hashAlgorithmFactory ?? throw new ArgumentNullException(nameof(hashAlgorithmFactory));
        }

        public string BuildStringToAppend(HeaderName header) {
            if (_request.Method == HttpMethod.Get) return string.Empty;
            
            if (_request.Content == null) return "\n" + new Header(HeaderName.PredefinedHeaderNames.Digest, string.Empty);
            
            var bodyText = _request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            using (var hashAlgorithm = _hashAlgorithmFactory.Create(_hashAlgorithm)) {
                var payloadBytes = hashAlgorithm.ComputeHash(bodyText);
                var digest = _base64Converter.ToBase64(payloadBytes);
                return "\n" + new Header(HeaderName.PredefinedHeaderNames.Digest, $"{hashAlgorithm.Name}={digest}");
            }
        }
    }
}