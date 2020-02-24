using System.Net.Http;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Signing {
    public interface IAdditionalSignatureHeadersSetter {
        Task AddMissingRequiredHeadersForSignature(HttpRequestMessage request, SigningSettings signingSettings);
    }
}