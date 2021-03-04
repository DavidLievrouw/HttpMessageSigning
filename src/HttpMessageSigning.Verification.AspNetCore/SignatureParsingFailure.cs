using System;

namespace Dalion.HttpMessageSigning.Verification.AspNetCore {
    /// <summary>
    /// Represents a parsing failure
    /// </summary>
    public class SignatureParsingFailure : SignatureParsingResult {
        /// <summary>
        /// Constructs a new parsing failure object 
        /// </summary>
        /// <param name="description">A description of the failure that occurred</param>
        /// <param name="failure">An optional exception to attach to this failure object</param>
        /// <exception cref="ArgumentException">Thrown if one of the parameters are invalid</exception>
        public SignatureParsingFailure(string description, Exception failure = null) {
            if (string.IsNullOrEmpty(description)) throw new ArgumentException("Value cannot be null or empty.", nameof(description));
            
            Description = description;
            Failure = failure;
        }
        
        /// <inheritdoc cref="IsSuccess"/>
        public override bool IsSuccess => false;

        /// <summary>
        /// A description of the failure
        /// </summary>
        public string Description { get; }
        
        /// <summary>
        /// An optional exception that was attached to this failure object
        /// </summary>
        public Exception Failure { get; }
    }
}