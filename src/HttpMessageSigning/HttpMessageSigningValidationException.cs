using System;
using System.Runtime.Serialization;

namespace Dalion.HttpMessageSigning {
    [Serializable]
    public class HttpMessageSigningValidationException : HttpMessageSigningException {
        private const string DefaultMessage = "An validation error has occurred while handling the HTTP request message signature.";
        
        public HttpMessageSigningValidationException() : base(DefaultMessage) { }
        public HttpMessageSigningValidationException(string message) : base(message) { }
        public HttpMessageSigningValidationException(string message, Exception innerException) : base(message, innerException) { }
        protected HttpMessageSigningValidationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public HttpMessageSigningValidationException(Exception inner) : base(DefaultMessage, inner) { }
    }
}