namespace Dalion.HttpMessageSigning.Verification {
    /// <summary>
    ///     A representation of a HTTP request message, that contains the data required for signature verification.
    /// </summary>
    public class HttpRequestForVerification : HttpRequestForSigning {
        /// <summary>
        ///     Gets or sets the binary representation of the body of the request.
        /// </summary>
        public byte[] Body { get; set; }

        /// <summary>
        ///     Gets or sets the parsed signature of the request.
        /// </summary>
        public Signature Signature { get; set; }

        /// <inheritdoc />
        public override object Clone() {
            return new HttpRequestForVerification {
                Method = Method,
                RequestUri = RequestUri,
                Body = (byte[]) Body?.Clone(),
                Headers = Headers == null ? null : new HeaderDictionary(Headers.ToDictionary()),
                Signature = (Signature) Signature?.Clone()
            };
        }
    }
}