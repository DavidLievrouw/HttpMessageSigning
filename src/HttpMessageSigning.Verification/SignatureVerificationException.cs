using System;
using System.Runtime.Serialization;

namespace Dalion.HttpMessageSigning.Verification {
    [Serializable]
    public class SignatureVerificationException : HttpMessageSigningException {
        private const string DefaultMessage = "The specified signature is invalid.";
        
        public SignatureVerificationException() : base(DefaultMessage) { }
        public SignatureVerificationException(string message) : base(message) { }
        public SignatureVerificationException(string message, Exception innerException) : base(message, innerException) { }
        protected SignatureVerificationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}