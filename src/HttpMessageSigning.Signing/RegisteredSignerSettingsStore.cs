using System;
using System.Collections.Generic;
using System.Linq;

namespace Dalion.HttpMessageSigning.Signing {
    internal class RegisteredSignerSettingsStore : IRegisteredSignerSettingsStore {
        private readonly IList<RegisteredSigningSettings> _registeredSigningSettings;

        public RegisteredSignerSettingsStore(IEnumerable<RegisteredSigningSettings> registeredSigningSettings) {
            _registeredSigningSettings = registeredSigningSettings?.ToList() ?? throw new ArgumentNullException(nameof(registeredSigningSettings));
        }

        public SigningSettings Get(KeyId keyId) {
            if (keyId == KeyId.Empty) throw new ArgumentException("The specified key id cannot be empty.", nameof(keyId));

            var match = _registeredSigningSettings.FirstOrDefault(_ => _.KeyId == keyId);
            if (match == null) {
                throw new InvalidOperationException($"No {nameof(IRequestSigner)} for {nameof(KeyId)} '{keyId}' has been registered.");
            }

            var signingSettings = match.SigningSettings;

            signingSettings.KeyId = keyId;
            
            return signingSettings;
        }

        public void Dispose() {
            foreach (var setting in _registeredSigningSettings) {
                setting?.Dispose();
            }
            _registeredSigningSettings.Clear();
        }
    }
}