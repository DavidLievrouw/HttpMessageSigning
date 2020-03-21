using System;
using System.Runtime.Serialization;

namespace Dalion.HttpMessageSigning.Verification {
    [Serializable]
    public class InvalidSignatureException : SignatureVerificationException {
        private const string DefaultMessage = "The specified client is invalid.";
        
        public InvalidSignatureException() : base(DefaultMessage) { }
        public InvalidSignatureException(string message) : base(message) { }
        public InvalidSignatureException(string message, Exception innerException) : base(message, innerException) { }
        protected InvalidSignatureException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}