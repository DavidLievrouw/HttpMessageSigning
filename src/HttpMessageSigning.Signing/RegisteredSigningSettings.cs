using System;

namespace Dalion.HttpMessageSigning.Signing {
    internal class RegisteredSigningSettings : IDisposable {
        public RegisteredSigningSettings(KeyId keyId, SigningSettings signingSettings) {
            KeyId = keyId;
            SigningSettings = signingSettings ?? throw new ArgumentNullException(nameof(signingSettings));
        }

        public KeyId KeyId { get; }
        public SigningSettings SigningSettings { get; }

        public void Dispose() {
            SigningSettings?.Dispose();
        }
    }
}