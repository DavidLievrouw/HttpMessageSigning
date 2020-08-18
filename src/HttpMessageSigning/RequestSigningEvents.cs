using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning {
    /// <summary>
    ///     Defines the available hooks during HTTP message signing.
    /// </summary>
    public class RequestSigningEvents {
        /// <summary>
        ///     Gets or sets the action to invoke just before signing a request.
        /// </summary>
        public Func<HttpRequestMessage, SigningSettings, Task> OnRequestSigning { get; set; } = (requestToSign, signingSettings) => Task.CompletedTask;

        /// <summary>
        ///     Gets or sets the action to invoke when the signing string has been composed.
        /// </summary>
        public Func<HttpRequestMessage, string, Task> OnSigningStringComposed { get; set; } = (requestToSign, signingString) => Task.CompletedTask;

        /// <summary>
        ///     Gets or sets the action to invoke when the request has been signed.
        /// </summary>
        public Func<HttpRequestMessage, Signature, SigningSettings, Task> OnRequestSigned { get; set; } = (signedRequest, signature, signingSettings) => Task.CompletedTask;
    }
}