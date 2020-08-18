using System;
using System.Runtime.Serialization;

namespace Dalion.HttpMessageSigning {
    /// <summary>
    ///     Represents an <see cref="Exception" /> that indicates that a HttpMessageSigning model validation operation has resulted in an error.
    /// </summary>
    [Serializable]
    public class ValidationException : HttpMessageSigningException {
        private const string DefaultMessage = "An validation error has occurred while handling the HTTP request message signature.";
        
        /// <inheritdoc />
        public ValidationException() : base(DefaultMessage) { }
        
        /// <inheritdoc />
        public ValidationException(string message) : base(message) { }
        
        /// <inheritdoc />
        public ValidationException(string message, Exception innerException) : base(message, innerException) { }
        
        /// <inheritdoc />
        protected ValidationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}