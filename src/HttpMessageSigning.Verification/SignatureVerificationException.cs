using System;

namespace Dalion.HttpMessageSigning.Verification {
    /// <summary>
    ///     Represents an <see cref="Exception" /> that indicates that a HttpMessageSigning verification operation has resulted in an error.
    /// </summary>
    [Serializable]
    public class SignatureVerificationException : HttpMessageSigningException {
        private const string DefaultMessage = "The specified signature is invalid.";
        
        /// <inheritdoc />
        public SignatureVerificationException() : base(DefaultMessage) { }
        
        /// <inheritdoc />
        public SignatureVerificationException(string message) : base(message) { }
        
        /// <inheritdoc />
        public SignatureVerificationException(string message, Exception innerException) : base(message, innerException) { }
    }
}