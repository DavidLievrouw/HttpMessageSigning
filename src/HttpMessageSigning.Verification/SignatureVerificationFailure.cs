using System;

namespace Dalion.HttpMessageSigning.Verification {
    /// <summary>
    /// Represents a failure of a request signature verification process.
    /// </summary>
    public abstract class SignatureVerificationFailure : IEquatable<SignatureVerificationFailure> {
        protected SignatureVerificationFailure(string code, string message, Exception exception) {
            if (string.IsNullOrEmpty(code)) throw new ArgumentException("Value cannot be null or empty.", nameof(code));
            Code = code;
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Exception = exception;
        }

        /// <summary>
        /// Gets the code that identifies the type of failure.
        /// </summary>
        public string Code { get; }
        
        /// <summary>
        /// Gets the informational message that describes the failure.
        /// </summary>
        public string Message { get; }
        
        /// <summary>
        /// Gets the exception that caused the failure.
        /// </summary>
        public Exception Exception { get; }

        public bool Equals(SignatureVerificationFailure other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Code == other.Code;
        }

        public override bool Equals(object obj) {
            return ReferenceEquals(this, obj) || obj is SignatureVerificationFailure other && Equals(other);
        }

        public override int GetHashCode() {
            return Code.GetHashCode();
        }

        public static bool operator ==(SignatureVerificationFailure left, SignatureVerificationFailure right) {
            return Equals(left, right);
        }

        public static bool operator !=(SignatureVerificationFailure left, SignatureVerificationFailure right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return string.IsNullOrEmpty(Message)
                ? $"{Code}"
                : $"{Code}: {Message}";
        }

        public static InvalidClientSignatureVerificationFailure InvalidClient(string message, Exception ex = null) {
            return new InvalidClientSignatureVerificationFailure(message, ex);
        }
        
        public static InvalidSignatureSignatureVerificationFailure InvalidSignature(string message, Exception ex = null) {
            return new InvalidSignatureSignatureVerificationFailure(message, ex);
        }
        
        public static HeaderMissingSignatureVerificationFailure HeaderMissing(string message, Exception ex = null) {
            return new HeaderMissingSignatureVerificationFailure(message, ex);
        }
        
        public static InvalidCreatedHeaderSignatureVerificationFailure InvalidCreatedHeader(string message, Exception ex = null) {
            return new InvalidCreatedHeaderSignatureVerificationFailure(message, ex);
        }
        
        public static InvalidExpiresHeaderSignatureVerificationFailure InvalidExpiresHeader(string message, Exception ex = null) {
            return new InvalidExpiresHeaderSignatureVerificationFailure(message, ex);
        }
        
        public static InvalidDigestHeaderSignatureVerificationFailure InvalidDigestHeader(string message, Exception ex = null) {
            return new InvalidDigestHeaderSignatureVerificationFailure(message, ex);
        }
        
        public static InvalidSignatureAlgorithmSignatureVerificationFailure InvalidSignatureAlgorithm(string message, Exception ex = null) {
            return new InvalidSignatureAlgorithmSignatureVerificationFailure(message, ex);
        }
        
        public static SignatureExpiredSignatureVerificationFailure SignatureExpired(string message, Exception ex = null) {
            return new SignatureExpiredSignatureVerificationFailure(message, ex);
        }
        
        public static InvalidSignatureStringSignatureVerificationFailure InvalidSignatureString(string message, Exception ex = null) {
            return new InvalidSignatureStringSignatureVerificationFailure(message, ex);
        }
        
        public static InvalidNonceSignatureVerificationFailure InvalidNonce(string message, Exception ex = null) {
            return new InvalidNonceSignatureVerificationFailure(message, ex);
        }
    }
}