using System;
using System.Security.Claims;

namespace Dalion.HttpMessageSigning.Validation {
    /// <summary>
    /// Represents a successful signature validation.
    /// </summary>
    public class RequestSignatureValidationResultSuccess : RequestSignatureValidationResult {
        internal RequestSignatureValidationResultSuccess(ClaimsPrincipal validatedPrincipal) {
            ValidatedPrincipal = validatedPrincipal ?? throw new ArgumentNullException(nameof(validatedPrincipal));
        }

        /// <summary>
        /// Gets the principal that represents the validated signature.
        /// </summary>
        public ClaimsPrincipal ValidatedPrincipal { get; }
        
        /// <summary>
        /// Gets a value indicating whether the signature was successfully validated.
        /// </summary>
        public override bool IsSuccess => true;
    }
}