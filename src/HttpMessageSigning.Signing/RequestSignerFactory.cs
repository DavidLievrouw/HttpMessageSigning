using System;
using Microsoft.Extensions.Logging;

namespace Dalion.HttpMessageSigning.Signing {
    internal class RequestSignerFactory : IRequestSignerFactory {
        private readonly ISignatureCreator _signatureCreator;
        private readonly IAuthorizationHeaderParamCreator _authorizationHeaderParamCreator;
        private readonly ISignatureHeaderEnsurer _dateHeaderEnsurer;
        private readonly ISignatureHeaderEnsurer _digestHeaderEnsurer;
        private readonly ISystemClock _systemClock;
        private readonly ILogger<RequestSigner> _logger;
        private readonly IRegisteredSignerSettingsStore _registeredSignerSettingsStore;

        public RequestSignerFactory(
            ISignatureCreator signatureCreator,
            IAuthorizationHeaderParamCreator authorizationHeaderParamCreator,
            ISignatureHeaderEnsurer dateHeaderEnsurer,
            ISignatureHeaderEnsurer digestHeaderEnsurer,
            ISystemClock systemClock,
            ILogger<RequestSigner> logger,
            IRegisteredSignerSettingsStore registeredSignerSettingsStore) {
            _signatureCreator = signatureCreator ?? throw new ArgumentNullException(nameof(signatureCreator));
            _authorizationHeaderParamCreator = authorizationHeaderParamCreator ?? throw new ArgumentNullException(nameof(authorizationHeaderParamCreator));
            _dateHeaderEnsurer = dateHeaderEnsurer ?? throw new ArgumentNullException(nameof(dateHeaderEnsurer));
            _digestHeaderEnsurer = digestHeaderEnsurer ?? throw new ArgumentNullException(nameof(digestHeaderEnsurer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _registeredSignerSettingsStore = registeredSignerSettingsStore ?? throw new ArgumentNullException(nameof(registeredSignerSettingsStore));
            _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
        }

        public IRequestSigner Create(KeyId keyId, SigningSettings signingSettings) {
            if (keyId == KeyId.Empty) throw new ArgumentException("The specified key id cannot be empty.", nameof(keyId));
            if (signingSettings == null) throw new ArgumentNullException(nameof(signingSettings));

            signingSettings.KeyId = keyId;

            signingSettings.Validate();

            return new RequestSigner(
                _signatureCreator,
                _authorizationHeaderParamCreator,
                signingSettings,
                _dateHeaderEnsurer,
                _digestHeaderEnsurer,
                _systemClock,
                _logger);
        }

        public IRequestSigner CreateFor(KeyId keyId) {
            if (keyId == KeyId.Empty) throw new ArgumentException("The specified key id cannot be empty.", nameof(keyId));

            var signingSettings = _registeredSignerSettingsStore.Get(keyId);
            if (signingSettings == null) {
                throw new InvalidOperationException($"No {nameof(IRequestSigner)} for {nameof(KeyId)} '{keyId}' has been registered.");
            }
            
            return Create(keyId, signingSettings);
        }

        public void Dispose() {
            _registeredSignerSettingsStore?.Dispose();
        }
    }
}