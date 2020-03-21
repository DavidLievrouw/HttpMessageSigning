using System;

namespace Dalion.HttpMessageSigning.Verification {
    public abstract class SignatureVerificationFailure : IEquatable<SignatureVerificationFailure> {
        protected SignatureVerificationFailure(string code, string message) {
            if (string.IsNullOrEmpty(code)) throw new ArgumentException("Value cannot be null or empty.", nameof(code));
            Code = code;
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        public string Code { get; }
        public string Message { get; }

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

        public static InvalidClientSignatureVerificationFailure InvalidClient(string message) {
            return new InvalidClientSignatureVerificationFailure(message);
        }
        
        public static InvalidSignatureSignatureVerificationFailure InvalidSignature(string message) {
            return new InvalidSignatureSignatureVerificationFailure(message);
        }
        
        public static HeaderMissingSignatureVerificationFailure HeaderMissing(string message) {
            return new HeaderMissingSignatureVerificationFailure(message);
        }
        
        public static InvalidCreatedHeaderSignatureVerificationFailure InvalidCreatedHeader(string message) {
            return new InvalidCreatedHeaderSignatureVerificationFailure(message);
        }
        
        public static InvalidExpiresHeaderSignatureVerificationFailure InvalidExpiresHeader(string message) {
            return new InvalidExpiresHeaderSignatureVerificationFailure(message);
        }
        
        public static InvalidDigestHeaderSignatureVerificationFailure InvalidDigestHeader(string message) {
            return new InvalidDigestHeaderSignatureVerificationFailure(message);
        }
        
        public static InvalidSignatureAlgorithmSignatureVerificationFailure InvalidSignatureAlgorithm(string message) {
            return new InvalidSignatureAlgorithmSignatureVerificationFailure(message);
        }
        
        public static SignatureExpiredSignatureVerificationFailure SignatureExpired(string message) {
            return new SignatureExpiredSignatureVerificationFailure(message);
        }
        
        public static InvalidSignatureStringSignatureVerificationFailure InvalidSignatureString(string message) {
            return new InvalidSignatureStringSignatureVerificationFailure(message);
        }
        
        public static InvalidNonceSignatureVerificationFailure InvalidNonce(string message) {
            return new InvalidNonceSignatureVerificationFailure(message);
        }
    }

    public class InvalidClientSignatureVerificationFailure : SignatureVerificationFailure {
        public InvalidClientSignatureVerificationFailure(string message) : base(
            "INVALID_CLIENT",
            message) {}
    }

    public class InvalidSignatureSignatureVerificationFailure : SignatureVerificationFailure {
        public InvalidSignatureSignatureVerificationFailure(string message) : base(
            "INVALID_SIGNATURE",
            message) {}
    }
    
    public class HeaderMissingSignatureVerificationFailure : SignatureVerificationFailure {
        public HeaderMissingSignatureVerificationFailure(string message) : base(
            "HEADER_MISSING",
            message) {}
    }
    
    public class InvalidSignatureStringSignatureVerificationFailure : SignatureVerificationFailure {
        public InvalidSignatureStringSignatureVerificationFailure(string message) : base(
            "INVALID_SIGNATURE_STRING",
            message) {}
    }
    
    public class InvalidCreatedHeaderSignatureVerificationFailure : SignatureVerificationFailure {
        public InvalidCreatedHeaderSignatureVerificationFailure(string message) : base(
            "INVALID_CREATED_HEADER",
            message) {}
    }
    
    public class InvalidExpiresHeaderSignatureVerificationFailure : SignatureVerificationFailure {
        public InvalidExpiresHeaderSignatureVerificationFailure(string message) : base(
            "INVALID_EXPIRES_HEADER",
            message) {}
    }
    
    public class SignatureExpiredSignatureVerificationFailure : SignatureVerificationFailure {
        public SignatureExpiredSignatureVerificationFailure(string message) : base(
            "SIGNATURE_EXPIRED",
            message) {}
    }
    
    public class InvalidDigestHeaderSignatureVerificationFailure : SignatureVerificationFailure {
        public InvalidDigestHeaderSignatureVerificationFailure(string message) : base(
            "INVALID_DIGEST_HEADER",
            message) {}
    }
    
    public class InvalidSignatureAlgorithmSignatureVerificationFailure : SignatureVerificationFailure {
        public InvalidSignatureAlgorithmSignatureVerificationFailure(string message) : base(
            "INVALID_SIGNATURE_ALGORITHM",
            message) {}
    }
    
    public class InvalidNonceSignatureVerificationFailure : SignatureVerificationFailure {
        public InvalidNonceSignatureVerificationFailure(string message) : base(
            "INVALID_NONCE",
            message) {}
    }
}