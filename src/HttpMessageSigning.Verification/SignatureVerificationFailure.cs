using System;

namespace Dalion.HttpMessageSigning.Verification {
    /// <summary>
    ///     Represents a failure of a request signature verification process.
    /// </summary>
    public abstract class SignatureVerificationFailure : IEquatable<SignatureVerificationFailure> {
        /// <summary>
        ///     Creates a new instance of this class.
        /// </summary>
        /// <param name="code">The code that identifies the type of failure.</param>
        /// <param name="message">The informational message that describes the failure.</param>
        /// <param name="exception">The exception that caused the failure, or a null reference if it is not caused by an exception.</param>
        /// <exception cref="ArgumentException"></exception>
        protected SignatureVerificationFailure(string code, string message, Exception exception) {
            if (string.IsNullOrEmpty(code)) throw new ArgumentException("Value cannot be null or empty.", nameof(code));
            Code = code;
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Exception = exception;
        }

        /// <summary>
        ///     Gets the code that identifies the type of failure.
        /// </summary>
        public string Code { get; }

        /// <summary>
        ///     Gets the informational message that describes the failure.
        /// </summary>
        public string Message { get; }

        /// <summary>
        ///     Gets the exception that caused the failure, or a null reference if it is not caused by an exception.
        /// </summary>
        public Exception Exception { get; }

        /// <inheritdoc />
        public bool Equals(SignatureVerificationFailure other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Code == other.Code;
        }

        /// <inheritdoc />
        public override bool Equals(object obj) {
            return ReferenceEquals(this, obj) || obj is SignatureVerificationFailure other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode() {
            return Code.GetHashCode();
        }

        /// <summary>
        ///     The equality operator of this type.
        /// </summary>
        /// <returns>True if the specified items are equal, otherwise false.</returns>
        public static bool operator ==(SignatureVerificationFailure left, SignatureVerificationFailure right) {
            return Equals(left, right);
        }

        /// <summary>
        ///     The inequality operator of this type.
        /// </summary>
        /// <returns>True if the specified items are not equal, otherwise false.</returns>
        public static bool operator !=(SignatureVerificationFailure left, SignatureVerificationFailure right) {
            return !Equals(left, right);
        }

        /// <inheritdoc />
        public override string ToString() {
            return string.IsNullOrEmpty(Message)
                ? $"{Code}"
                : $"{Code}: {Message}";
        }

        /// <summary>
        ///     Represents a failure that is caused by specifying an invalid client.
        /// </summary>
        /// <param name="message">The informational message that describes the failure.</param>
        /// <param name="ex">The exception that caused the failure, or a null reference if it is not caused by an exception.</param>
        /// <returns>The newly created failure.</returns>
        public static InvalidClientSignatureVerificationFailure InvalidClient(string message, Exception ex = null) {
            return new InvalidClientSignatureVerificationFailure(message, ex);
        }

        /// <summary>
        ///     Represents a failure that is caused by specifying an invalid signature.
        /// </summary>
        /// <param name="message">The informational message that describes the failure.</param>
        /// <param name="ex">The exception that caused the failure, or a null reference if it is not caused by an exception.</param>
        /// <returns>The newly created failure.</returns>
        public static InvalidSignatureSignatureVerificationFailure InvalidSignature(string message, Exception ex = null) {
            return new InvalidSignatureSignatureVerificationFailure(message, ex);
        }

        /// <summary>
        ///     Represents a failure that is caused by a missing header that is part of the signature.
        /// </summary>
        /// <param name="message">The informational message that describes the failure.</param>
        /// <param name="ex">The exception that caused the failure, or a null reference if it is not caused by an exception.</param>
        /// <returns>The newly created failure.</returns>
        public static HeaderMissingSignatureVerificationFailure HeaderMissing(string message, Exception ex = null) {
            return new HeaderMissingSignatureVerificationFailure(message, ex);
        }

        /// <summary>
        ///     Represents a failure that is caused by an invalid value of the (created) pseudo-header.
        /// </summary>
        /// <param name="message">The informational message that describes the failure.</param>
        /// <param name="ex">The exception that caused the failure, or a null reference if it is not caused by an exception.</param>
        /// <returns>The newly created failure.</returns>
        public static InvalidCreatedHeaderSignatureVerificationFailure InvalidCreatedHeader(string message, Exception ex = null) {
            return new InvalidCreatedHeaderSignatureVerificationFailure(message, ex);
        }

        /// <summary>
        ///     Represents a failure that is caused by an invalid value of the (expires) pseudo-header.
        /// </summary>
        /// <param name="message">The informational message that describes the failure.</param>
        /// <param name="ex">The exception that caused the failure, or a null reference if it is not caused by an exception.</param>
        /// <returns>The newly created failure.</returns>
        public static InvalidExpiresHeaderSignatureVerificationFailure InvalidExpiresHeader(string message, Exception ex = null) {
            return new InvalidExpiresHeaderSignatureVerificationFailure(message, ex);
        }

        /// <summary>
        ///     Represents a failure that is caused by an invalid value of the digest header.
        /// </summary>
        /// <param name="message">The informational message that describes the failure.</param>
        /// <param name="ex">The exception that caused the failure, or a null reference if it is not caused by an exception.</param>
        /// <returns>The newly created failure.</returns>
        public static InvalidDigestHeaderSignatureVerificationFailure InvalidDigestHeader(string message, Exception ex = null) {
            return new InvalidDigestHeaderSignatureVerificationFailure(message, ex);
        }

        /// <summary>
        ///     Represents a failure that is caused by an invalid signature algorithm.
        /// </summary>
        /// <param name="message">The informational message that describes the failure.</param>
        /// <param name="ex">The exception that caused the failure, or a null reference if it is not caused by an exception.</param>
        /// <returns>The newly created failure.</returns>
        public static InvalidSignatureAlgorithmSignatureVerificationFailure InvalidSignatureAlgorithm(string message, Exception ex = null) {
            return new InvalidSignatureAlgorithmSignatureVerificationFailure(message, ex);
        }

        /// <summary>
        ///     Represents a failure that is caused by the signature being expired.
        /// </summary>
        /// <param name="message">The informational message that describes the failure.</param>
        /// <param name="ex">The exception that caused the failure, or a null reference if it is not caused by an exception.</param>
        /// <returns>The newly created failure.</returns>
        public static SignatureExpiredSignatureVerificationFailure SignatureExpired(string message, Exception ex = null) {
            return new SignatureExpiredSignatureVerificationFailure(message, ex);
        }

        /// <summary>
        ///     Represents a failure that is caused by an invalid signature string.
        /// </summary>
        /// <param name="message">The informational message that describes the failure.</param>
        /// <param name="ex">The exception that caused the failure, or a null reference if it is not caused by an exception.</param>
        /// <returns>The newly created failure.</returns>
        public static InvalidSignatureStringSignatureVerificationFailure InvalidSignatureString(string message, Exception ex = null) {
            return new InvalidSignatureStringSignatureVerificationFailure(message, ex);
        }

        /// <summary>
        ///     Represents a failure that is caused by a replayed request.
        /// </summary>
        /// <param name="message">The informational message that describes the failure.</param>
        /// <param name="ex">The exception that caused the failure, or a null reference if it is not caused by an exception.</param>
        /// <returns>The newly created failure.</returns>
        public static ReplayedRequestSignatureVerificationFailure ReplayedRequest(string message, Exception ex = null) {
            return new ReplayedRequestSignatureVerificationFailure(message, ex);
        }
    }
}