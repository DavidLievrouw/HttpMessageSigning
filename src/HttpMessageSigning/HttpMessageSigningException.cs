using System;
using System.Runtime.Serialization;

namespace Dalion.HttpMessageSigning {
    [Serializable]
    public class HttpMessageSigningException : Exception {
        private const string DefaultMessage = "An error has occurred while handling the HTTP request message signature.";
        
        public HttpMessageSigningException() : base(DefaultMessage) { }
        public HttpMessageSigningException(string message) : base(message) { }
        public HttpMessageSigningException(string message, Exception innerException) : base(message, innerException) { }
        protected HttpMessageSigningException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}