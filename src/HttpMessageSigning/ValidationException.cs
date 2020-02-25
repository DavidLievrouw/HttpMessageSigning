using System;
using System.Runtime.Serialization;

namespace Dalion.HttpMessageSigning {
    [Serializable]
    public class ValidationException : HttpMessageSigningException {
        private const string DefaultMessage = "An validation error has occurred while handling the HTTP request message signature.";
        
        public ValidationException() : base(DefaultMessage) { }
        public ValidationException(string message) : base(message) { }
        public ValidationException(string message, Exception innerException) : base(message, innerException) { }
        protected ValidationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public ValidationException(Exception inner) : base(DefaultMessage, inner) { }
    }
}