using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Signing {
    internal interface ISignatureCreator {
        Task<Signature> CreateSignature(HttpRequestMessage request, SigningSettings settings, DateTimeOffset timeOfSigning, TimeSpan expires);
    }
}