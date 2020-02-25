using System;
using System.Runtime.Serialization;

namespace Dalion.HttpMessageSigning.Validation {
    [Serializable]
    public class HttpMessageSigningSignatureValidationException : HttpMessageSigningException {
        private const string DefaultMessage = "The specified signature is invalid.";
        
        public HttpMessageSigningSignatureValidationException() : base(DefaultMessage) { }
        public HttpMessageSigningSignatureValidationException(string message) : base(message) { }
        public HttpMessageSigningSignatureValidationException(string message, Exception innerException) : base(message, innerException) { }
        protected HttpMessageSigningSignatureValidationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public HttpMessageSigningSignatureValidationException(Exception inner) : base(DefaultMessage, inner) { }
    }
}