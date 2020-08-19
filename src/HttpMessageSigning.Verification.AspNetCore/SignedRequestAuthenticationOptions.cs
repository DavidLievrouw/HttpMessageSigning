using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Dalion.HttpMessageSigning.Verification.AspNetCore {
    /// <summary>
    ///     Options for the authentication scheme.
    /// </summary>
    public class SignedRequestAuthenticationOptions : AuthenticationSchemeOptions {
        /// <summary>
        ///     Gets or sets the realm of the authentication scheme.
        /// </summary>
        public string Realm { get; set; }

        /// <summary>
        ///     Gets or sets the name of the authentication scheme.
        /// </summary>
        public string Scheme { get; set; } = SignedHttpRequestDefaults.AuthenticationScheme;

        /// <summary>
        ///     Gets or sets the action to invoke when a request signature has been parsed.
        /// </summary>
        public Func<HttpRequest, Signature, Task> OnSignatureParsed { get; set; }

        /// <summary>
        ///     Gets or sets the action to invoke when an identity has been verified by this authentication scheme.
        /// </summary>
        public Func<HttpRequest, RequestSignatureVerificationResultSuccess, Task> OnIdentityVerified { get; set; }

        /// <summary>
        ///     Gets or sets the action to invoke when identity verification failed.
        /// </summary>
        public Func<HttpRequest, RequestSignatureVerificationResultFailure, Task> OnIdentityVerificationFailed { get; set; }
    }
}