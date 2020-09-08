using System;
using System.Net.Http;
using Dalion.HttpMessageSigning.SigningString;

namespace Dalion.HttpMessageSigning.Verification {
    /// <summary>
    ///     A representation of a HTTP request message, that contains the data required for signature verification.
    /// </summary>
    public class HttpRequestForVerification : ICloneable {
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
        
        /// <summary>
        ///     Gets or sets the binary representation of the body of the request.
        /// </summary>
        public byte[] Body { get; set; }

        /// <summary>
        ///     Gets or sets the parsed signature of the request.
        /// </summary>
        public Signature Signature { get; set; }

        /// <inheritdoc />
        public object Clone() {
            return new HttpRequestForVerification {
                Method = Method,
                RequestUri = RequestUri,
                Body = (byte[]) Body?.Clone(),
                Headers = Headers == null ? null : new HeaderDictionary(Headers.ToDictionary()),
                Signature = (Signature) Signature?.Clone()
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