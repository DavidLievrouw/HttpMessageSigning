using System;

namespace Dalion.HttpMessageSigning.Verification.AspNetCore {
    /// <summary>
    /// Represents a successful result in the parsing of a signature header
    /// </summary>
    public class SignatureParsingSuccess : SignatureParsingResult {
        
        /// <summary>
        /// Constructs a new parsing success object 
        /// </summary>
        /// <param name="signature">The signature that was parsed</param>
        public SignatureParsingSuccess(Signature signature) {
            Signature = signature ?? throw new ArgumentNullException(nameof(signature));
        }

        /// <inheritdoc cref="IsSuccess"/>
        public override bool IsSuccess => true;

        /// <summary>
        /// The parsed signature
        /// </summary>
        public Signature Signature { get; }
    }
}