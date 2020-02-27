using System;

namespace Dalion.HttpMessageSigning.Signing {
    internal interface IRegisteredSignerSettingsStore : IDisposable {
        SigningSettings Get(KeyId keyId);
    }
}