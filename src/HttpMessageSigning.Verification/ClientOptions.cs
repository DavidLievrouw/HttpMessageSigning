using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Dalion.HttpMessageSigning.Verification {
    /// <summary>
    ///     Represents options for the creation of <see cref="Client" /> instances.
    /// </summary>
    public class ClientOptions : IValidatable {
        private static readonly TimeSpan DefaultNonceLifetime = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan DefaultClockSkew = TimeSpan.FromMinutes(1);

        /// <summary>
        ///     Gets an instance of the <see cref="ClientOptions" /> class, with the default property values.
        /// </summary>
        public static ClientOptions Default { get; } = new ClientOptions();

        /// <summary>
        ///     Gets or sets the time span after which repeated nonce values are allowed again.
        /// </summary>
        public TimeSpan NonceLifetime { get; set; } = DefaultNonceLifetime;

        /// <summary>
        ///     Gets or sets the clock skew to allow when validation a time.
        /// </summary>
        public TimeSpan ClockSkew { get; set; } = DefaultClockSkew;

        /// <summary>
        ///     Gets or sets the method of escaping the value of the (request-target) pseudo-header.
        /// </summary>
        public RequestTargetEscaping RequestTargetEscaping { get; set; } = RequestTargetEscaping.RFC3986;

        /// <summary>
        ///     Gets or sets the additional claims that the validated principal will have upon successful signature verification.
        /// </summary>
        public Claim[] Claims { get; set; } = Array.Empty<Claim>();

        /// <inheritdoc />
        public void Validate() {
            var error = GetValidationErrors().FirstOrDefault();
            if (error != null) throw new ValidationException(error.Message);
        }

        /// <inheritdoc />
        public IEnumerable<ValidationError> GetValidationErrors() {
            var errors = new List<ValidationError>();

            if (NonceLifetime <= TimeSpan.Zero) errors.Add(new ValidationError(nameof(NonceLifetime), $"The specified {nameof(NonceLifetime)} value cannot be zero or negative."));
            if (ClockSkew <= TimeSpan.Zero) errors.Add(new ValidationError(nameof(ClockSkew), $"The specified {nameof(ClockSkew)} value cannot be zero or negative."));
            if (!Enum.IsDefined(typeof(RequestTargetEscaping), RequestTargetEscaping)) {
                errors.Add(new ValidationError(nameof(RequestTargetEscaping), $"The specified {nameof(RequestTargetEscaping)} value is not supported."));
            }

            return errors;
        }
    }
}