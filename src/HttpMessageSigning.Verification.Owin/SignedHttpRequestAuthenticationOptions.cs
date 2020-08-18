using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Security;

namespace Dalion.HttpMessageSigning.Verification.Owin {
    /// <summary>
    ///     The <see cref="AuthenticationOptions" /> for the HttpMessageSigning Owin authentication middleware.
    /// </summary>
    public class SignedHttpRequestAuthenticationOptions : AuthenticationOptions {
        /// <inheritdoc />
        public SignedHttpRequestAuthenticationOptions() : base(SignedHttpRequestDefaults.AuthenticationScheme) { }

        /// <summary>
        ///     Gets or sets the <see cref="IRequestSignatureVerifier" /> that will verify signatures of incoming requests.
        /// </summary>
        public IRequestSignatureVerifier RequestSignatureVerifier { get; set; }

        /// <summary>
        ///     Gets or sets the name of the authentication scheme.
        /// </summary>
        public string Scheme { get; set; } = SignedHttpRequestDefaults.AuthenticationScheme;

        /// <summary>
        ///     Gets or sets the realm of the authentication scheme.
        /// </summary>
        public string Realm { get; set; }

        /// <summary>
        ///     Gets or sets the action to invoke when an identity has been verified by this authentication scheme.
        /// </summary>
        public Func<IOwinRequest, RequestSignatureVerificationResultSuccess, Task> OnIdentityVerified { get; set; }

        /// <summary>
        ///     Gets or sets the action to invoke when identity verification failed.
        /// </summary>
        public Func<IOwinRequest, RequestSignatureVerificationResultFailure, Task> OnIdentityVerificationFailed { get; set; }

        /// <summary>
        ///     Gets or sets the action to invoke when a request signature has been parsed.
        /// </summary>
        public Func<IOwinRequest, Signature, Task> OnSignatureParsed { get; set; }

        internal void Validate() {
            if (string.IsNullOrEmpty(Scheme)) throw new ValidationException($"The {nameof(SignedHttpRequestAuthenticationOptions)} do not specify a valid {nameof(Scheme)}.");
            if (string.IsNullOrEmpty(Realm)) throw new ValidationException($"The {nameof(SignedHttpRequestAuthenticationOptions)} do not specify a valid {nameof(Realm)}.");
            if (RequestSignatureVerifier == null)
                throw new ValidationException($"The {nameof(SignedHttpRequestAuthenticationOptions)} do not specify a valid {nameof(RequestSignatureVerifier)}.");
            if (Scheme.Contains(" ")) throw new ValidationException($"{nameof(Scheme)} cannot contain whitespace.");
        }
    }
}