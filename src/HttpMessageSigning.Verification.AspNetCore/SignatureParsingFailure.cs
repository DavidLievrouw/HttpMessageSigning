using System;

namespace Dalion.HttpMessageSigning.Verification.AspNetCore {
    /// <summary>
    ///     Represents an unsuccessful result of a <see cref="ISignatureParser" />.
    /// </summary>
    public class SignatureParsingFailure : SignatureParsingResult {
        /// <summary>
        ///     Creates a new instance of this class.
        /// </summary>
        /// <param name="description">The description of the cause of the failure.</param>
        /// <param name="failure">The <see cref="Exception" /> that caused the failure, if any.</param>
        public SignatureParsingFailure(string description, Exception failure = null) {
            if (string.IsNullOrEmpty(description)) throw new ArgumentException("Value cannot be null or empty.", nameof(description));

            Description = description;
            Failure = failure;
        }

        /// <inheritdoc />
        public override bool IsSuccess => false;

        /// <summary>
        ///     Gets the description of the cause of the failure.
        /// </summary>
        public string Description { get; }

        /// <summary>
        ///     Gets the <see cref="Exception" /> that caused the failure, or <see langword="null" /> if no <see cref="Exception" /> occurred.
        /// </summary>
        public Exception Failure { get; }
    }
}