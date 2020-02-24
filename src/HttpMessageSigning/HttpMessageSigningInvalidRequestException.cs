using System;
using System.Runtime.Serialization;

namespace Dalion.HttpMessageSigning {
    [Serializable]
    public class HttpMessageSigningInvalidRequestException : HttpMessageSigningException {
        private const string DefaultMessage = "The specified request cannot be used for signing.";
        
        public HttpMessageSigningInvalidRequestException() : base(DefaultMessage) { }
        public HttpMessageSigningInvalidRequestException(string message) : base(message) { }
        public HttpMessageSigningInvalidRequestException(string message, Exception innerException) : base(message, innerException) { }
        protected HttpMessageSigningInvalidRequestException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public HttpMessageSigningInvalidRequestException(Exception inner) : base(DefaultMessage, inner) { }
    }
}