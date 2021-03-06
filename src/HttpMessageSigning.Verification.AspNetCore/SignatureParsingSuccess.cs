using System;

namespace Dalion.HttpMessageSigning.Verification.AspNetCore {
    /// <summary>
    ///     Represents a successful result of a <see cref="ISignatureParser" />.
    /// </summary>
    public class SignatureParsingSuccess : SignatureParsingResult {
        /// <summary>
        ///     Creates a new instance of this class.
        /// </summary>
        /// <param name="signature">The successfully parsed <see cref="Signature" />.</param>
        public SignatureParsingSuccess(Signature signature) {
            Signature = signature ?? throw new ArgumentNullException(nameof(signature));
        }

        /// <inheritdoc />
        public override bool IsSuccess => true;

        /// <summary>
        ///     Gets the successfully parsed <see cref="Signature" />.
        /// </summary>
        public Signature Signature { get; }
    }
}