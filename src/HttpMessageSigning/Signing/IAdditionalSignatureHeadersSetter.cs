using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Signing {
    internal interface IAdditionalSignatureHeadersSetter {
        Task AddMissingRequiredHeadersForSignature(HttpRequestMessage request, SigningSettings signingSettings, DateTimeOffset timeOfSigning);
    }
}