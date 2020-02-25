using System;
using System.Runtime.Serialization;

namespace Dalion.HttpMessageSigning.Validation {
    [Serializable]
    public class SignatureValidationException : HttpMessageSigningException {
        private const string DefaultMessage = "The specified signature is invalid.";
        
        public SignatureValidationException() : base(DefaultMessage) { }
        public SignatureValidationException(string message) : base(message) { }
        public SignatureValidationException(string message, Exception innerException) : base(message, innerException) { }
        protected SignatureValidationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public SignatureValidationException(Exception inner) : base(DefaultMessage, inner) { }
    }
}