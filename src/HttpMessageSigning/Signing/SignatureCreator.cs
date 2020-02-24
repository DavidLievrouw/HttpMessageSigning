using System;
using System.Net.Http;
using System.Text;
using Dalion.HttpMessageSigning.SigningString;

namespace Dalion.HttpMessageSigning.Signing {
    internal class SignatureCreator : ISignatureCreator {
        private readonly ISigningStringComposer _signingStringComposer;
        private readonly IKeyedHashAlgorithmFactory _keyedHashAlgorithmFactory;
        private readonly IBase64Converter _base64Converter;
        private readonly ISystemClock _systemClock;

        public SignatureCreator(
            ISigningStringComposer signingStringComposer,
            IKeyedHashAlgorithmFactory keyedHashAlgorithmFactory,
            IBase64Converter base64Converter,
            ISystemClock systemClock) {
            _signingStringComposer = signingStringComposer ?? throw new ArgumentNullException(nameof(signingStringComposer));
            _keyedHashAlgorithmFactory = keyedHashAlgorithmFactory ?? throw new ArgumentNullException(nameof(keyedHashAlgorithmFactory));
            _base64Converter = base64Converter ?? throw new ArgumentNullException(nameof(base64Converter));
            _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
        }

        public Signature CreateSignature(HttpRequestMessage request, SigningSettings settings) {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            settings.Validate();

            var timeOfComposing = _systemClock.UtcNow;
            var signingString = _signingStringComposer.Compose(request, settings, timeOfComposing);

            var signingKey = _base64Converter.FromBase64(settings.KeyId.Key);
            var hashAlgorithm = _keyedHashAlgorithmFactory.Create(settings.KeyId.SignatureAlgorithm, settings.KeyId.HashAlgorithm, signingKey);

            var signatureHash = hashAlgorithm.ComputeHash(signingString);
            var signatureString = _base64Converter.ToBase64(signatureHash);

            var signature = new Signature {
                KeyId = settings.KeyId,
                Algorithm = settings.Algorithm,
                Created = timeOfComposing,
                Expires = timeOfComposing.Add(settings.Expires),
                Headers = settings.Headers,
                String = signatureString
            };

            return signature;
        }
    }
}