using System;
using System.Net.Http;
using Dalion.HttpMessageSigning.SigningString;

namespace Dalion.HttpMessageSigning {
    /// <summary>
    ///     A representation of a HTTP request message, that contains the data required for signing.
    /// </summary>
    public class HttpRequestForSigning : ICloneable {
        /// <summary>
        ///     Gets or sets the HTTP method of the request.
        /// </summary>
        public HttpMethod Method { get; set; } = HttpMethod.Get;

        /// <summary>
        ///     Gets or sets the target URI of the request.
        /// </summary>
        public Uri RequestUri { get; set; }

        /// <summary>
        ///     Gets or sets the headers of the request.
        /// </summary>
        public HeaderDictionary Headers { get; set; } = new HeaderDictionary();

        /// <inheritdoc />
        public object Clone() {
            return new HttpRequestForSigning {
                Method = Method,
                RequestUri = RequestUri,
                Headers = Headers == null ? null : new HeaderDictionary(Headers.ToDictionary())
            };
        }

        internal HttpRequestForSignatureString ToHttpRequestForSignatureString() {
            return new HttpRequestForSignatureString {
                Method = Method,
                RequestUri = RequestUri,
                Headers = Headers == null ? null : new HeaderDictionary(Headers.ToDictionary())
            };
        }
    }
}