using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning {
    public class RequestSigningEvents {
        public Func<HttpRequestMessage, SigningSettings, Task> OnRequestSigning { get; set; } = (requestToSign, signingSettings) => Task.CompletedTask;
        public Func<HttpRequestMessage, string, Task> OnSigningStringComposed { get; set; } = (requestToSign, signatureString) => Task.CompletedTask;
        public Func<HttpRequestMessage, Signature, SigningSettings, Task> OnRequestSigned { get; set; } = (signedRequest, signature, signingSettings) => Task.CompletedTask;
    }
}