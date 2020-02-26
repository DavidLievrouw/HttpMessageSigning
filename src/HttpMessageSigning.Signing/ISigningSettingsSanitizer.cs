using System.Net.Http;

namespace Dalion.HttpMessageSigning.Signing {
    internal interface ISigningSettingsSanitizer {
        void SanitizeHeaderNamesToInclude(SigningSettings signingSettings, HttpRequestMessage request);
    }
}