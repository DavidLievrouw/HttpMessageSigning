using System;
using System.Net.Http;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.SigningString;
using Microsoft.Extensions.Logging;

namespace Dalion.HttpMessageSigning.Signing {
    internal class SignatureCreator : ISignatureCreator {
        private readonly ISigningStringComposer _signingStringComposer;
        private readonly IBase64Converter _base64Converter;
        private readonly ISigningStringCompositionRequestFactory _stringCompositionRequestFactory;
        private readonly ILogger<SignatureCreator> _logger;

        public SignatureCreator(
            ISigningStringComposer signingStringComposer,
            IBase64Converter base64Converter,
            ISigningStringCompositionRequestFactory stringCompositionRequestFactory,
            ILogger<SignatureCreator> logger = null) {
            _signingStringComposer = signingStringComposer ?? throw new ArgumentNullException(nameof(signingStringComposer));
            _base64Converter = base64Converter ?? throw new ArgumentNullException(nameof(base64Converter));
            _stringCompositionRequestFactory = stringCompositionRequestFactory ?? throw new ArgumentNullException(nameof(stringCompositionRequestFactory));
            _logger = logger;
        }

        public async Task<Signature> CreateSignature(HttpRequestMessage request, SigningSettings settings, DateTimeOffset timeOfSigning, TimeSpan expires) {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            
            var requestForSigning = request.ToHttpRequestForSigning();
            var compositionRequest = _stringCompositionRequestFactory.CreateForSigning(requestForSigning, settings, timeOfSigning, expires);
            var signingString = _signingStringComposer.Compose(compositionRequest);

            var eventTask = settings.Events?.OnSigningStringComposed?.Invoke(request, ref signingString);
            if (eventTask != null) await eventTask.ConfigureAwait(continueOnCapturedContext: false);

            _logger?.LogDebug("Composed the following signing string for request signing: {0}", signingString);

            var signatureHash = settings.SignatureAlgorithm.ComputeHash(signingString);
            var signatureString = _base64Converter.ToBase64(signatureHash);

            _logger?.LogDebug("The base64 hash of the signature string for signing is '{0}'.", signatureString);

            var signature = new Signature {
                KeyId = settings.KeyId,
                Algorithm = settings.UseDeprecatedAlgorithmParameter
                    ? $"{settings.SignatureAlgorithm.Name.ToLower()}-{settings.SignatureAlgorithm.HashAlgorithm.ToString().ToLower()}"
                    : Signature.DefaultSignatureAlgorithm,
                Created = timeOfSigning,
                Expires = timeOfSigning.Add(expires),
                Headers = settings.Headers,
                Nonce = compositionRequest.Nonce,
                String = signatureString
            };

            return signature;
        }
    }
}