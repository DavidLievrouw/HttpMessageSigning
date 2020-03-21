using System;
using System.Runtime.Serialization;

namespace Dalion.HttpMessageSigning.Verification {
    [Serializable]
    public class InvalidClientException : SignatureVerificationException {
        private const string DefaultMessage = "The specified client is invalid.";
        
        public InvalidClientException() : base(DefaultMessage) { }
        public InvalidClientException(string message) : base(message) { }
        public InvalidClientException(string message, Exception innerException) : base(message, innerException) { }
        protected InvalidClientException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}