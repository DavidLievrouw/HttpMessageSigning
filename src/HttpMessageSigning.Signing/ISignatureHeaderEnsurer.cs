using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Signing {
    internal interface ISignatureHeaderEnsurer {
        Task EnsureHeader(HttpRequestMessage request, SigningSettings signingSettings, DateTimeOffset timeOfSigning);
    }
}