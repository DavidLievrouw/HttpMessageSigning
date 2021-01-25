using System;
using Dalion.HttpMessageSigning.Utils;
using Microsoft.Extensions.Logging;

namespace Dalion.HttpMessageSigning.Signing {
    internal class RequestSignerFactory : IRequestSignerFactory {
        private readonly ISigningSettingsSanitizer _signingSettingsSanitizer;
        private readonly ISignatureCreator _signatureCreator;
        private readonly IAuthorizationHeaderParamCreator _authorizationHeaderParamCreator;
        private readonly ISignatureHeaderEnsurer _signatureHeaderEnsurer;
        private readonly ISystemClock _systemClock;
        private readonly ILogger<RequestSigner> _logger;
        private readonly IRegisteredSignerSettingsStore _registeredSignerSettingsStore;

        public RequestSignerFactory(
            ISigningSettingsSanitizer signingSettingsSanitizer,
            ISignatureCreator signatureCreator,
            IAuthorizationHeaderParamCreator authorizationHeaderParamCreator,
            ISignatureHeaderEnsurer signatureHeaderEnsurer,
            ISystemClock systemClock,
            IRegisteredSignerSettingsStore registeredSignerSettingsStore,
            ILogger<RequestSigner> logger = null) {
            _signingSettingsSanitizer = signingSettingsSanitizer ?? throw new ArgumentNullException(nameof(signingSettingsSanitizer));
            _signatureCreator = signatureCreator ?? throw new ArgumentNullException(nameof(signatureCreator));
            _authorizationHeaderParamCreator = authorizationHeaderParamCreator ?? throw new ArgumentNullException(nameof(authorizationHeaderParamCreator));
            _signatureHeaderEnsurer = signatureHeaderEnsurer ?? throw new ArgumentNullException(nameof(signatureHeaderEnsurer));
            _logger = logger;
            _registeredSignerSettingsStore = registeredSignerSettingsStore ?? throw new ArgumentNullException(nameof(registeredSignerSettingsStore));
            _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
        }

        public IRequestSigner Create(KeyId keyId, SigningSettings signingSettings) {
            if (keyId == KeyId.Empty) throw new ArgumentException("The specified key id cannot be empty.", nameof(keyId));
            if (signingSettings == null) throw new ArgumentNullException(nameof(signingSettings));

            signingSettings.KeyId = keyId;

            signingSettings.Validate();
            
            return new RequestSigner(
                _signingSettingsSanitizer,
                _signatureCreator,
                _authorizationHeaderParamCreator,
                signingSettings,
                _signatureHeaderEnsurer,
                _systemClock,
                _logger);
        }

        public IRequestSigner CreateFor(KeyId keyId) {
            if (keyId == KeyId.Empty) throw new ArgumentException("The specified key id cannot be empty.", nameof(keyId));

            var signingSettings = _registeredSignerSettingsStore.Get(keyId);
            if (signingSettings == null) {
                throw new InvalidOperationException($"No {nameof(IRequestSigner)} for {nameof(KeyId)} '{keyId}' has been registered.");
            }
            
            signingSettings.Validate();
            
            return Create(keyId, signingSettings);
        }

        public void Dispose() {
            _registeredSignerSettingsStore?.Dispose();
        }
    }
}