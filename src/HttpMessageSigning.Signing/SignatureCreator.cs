using System;
using System.Net.Http;
using Dalion.HttpMessageSigning.Logging;
using Dalion.HttpMessageSigning.SigningString;

namespace Dalion.HttpMessageSigning.Signing {
    internal class SignatureCreator : ISignatureCreator {
        private readonly ISigningStringComposer _signingStringComposer;
        private readonly IBase64Converter _base64Converter;
        private readonly IHttpMessageSigningLogger<SignatureCreator> _logger;

        public SignatureCreator(
            ISigningStringComposer signingStringComposer,
            IBase64Converter base64Converter,
            IHttpMessageSigningLogger<SignatureCreator> logger) {
            _signingStringComposer = signingStringComposer ?? throw new ArgumentNullException(nameof(signingStringComposer));
            _base64Converter = base64Converter ?? throw new ArgumentNullException(nameof(base64Converter));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Signature CreateSignature(HttpRequestMessage request, SigningSettings settings, DateTimeOffset timeOfSigning) {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            settings.Validate();

            var requestForSigning = request.ToRequestForSigning();
            var signingString = _signingStringComposer.Compose(requestForSigning, settings, timeOfSigning);

            _logger.Debug("Composed the following string for request signing: {0}", signingString);

            var signatureHash = settings.SignatureAlgorithm.ComputeHash(signingString);
            var signatureString = _base64Converter.ToBase64(signatureHash);

            _logger.Debug("The base64 hash of the signature string is {0}", signatureString);

            var signature = new Signature {
                KeyId = settings.KeyId,
                Algorithm = $"{settings.SignatureAlgorithm.Name.ToLowerInvariant()}-{settings.SignatureAlgorithm.HashAlgorithm.ToString().ToLowerInvariant()}",
                Created = timeOfSigning,
                Expires = timeOfSigning.Add(settings.Expires),
                Headers = settings.Headers,
                String = signatureString
            };

            return signature;
        }
    }
}