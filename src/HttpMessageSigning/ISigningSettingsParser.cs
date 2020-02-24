using System.Net.Http;

namespace Dalion.HttpMessageSigning {
    public interface ISigningSettingsParser {
        SigningSettings Parse(HttpRequestMessage request);
    }
}