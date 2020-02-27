using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning {
    public class RequestSigningEvents {
        public Func<HttpRequestMessage, SigningSettings, Task> OnRequestSigning { get; set; } = (requestToSign, signingSettings) => Task.CompletedTask;
        public Func<HttpRequestMessage, SigningSettings, Task> OnRequestSigned { get; set; } = (signedRequest, signingSettings) => Task.CompletedTask;
    }
}