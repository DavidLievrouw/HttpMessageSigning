using System;
using System.Runtime.Serialization;

namespace Dalion.HttpMessageSigning {
    /// <summary>
    ///     Represents an <see cref="Exception" /> that indicates that a HttpMessageSigning operation has resulted in an error.
    /// </summary>
    [Serializable]
    public class HttpMessageSigningException : Exception {
        private const string DefaultMessage = "An error has occurred while handling the HTTP request message signature.";

        /// <inheritdoc />
        public HttpMessageSigningException() : base(DefaultMessage) { }

        /// <inheritdoc />
        public HttpMessageSigningException(string message) : base(message) { }

        /// <inheritdoc />
        public HttpMessageSigningException(string message, Exception innerException) : base(message, innerException) { }

        /// <inheritdoc />
        protected HttpMessageSigningException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}