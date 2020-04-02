using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Security;

namespace Dalion.HttpMessageSigning.Verification.Owin {
    public class SignedHttpRequestAuthenticationOptions : AuthenticationOptions {
        public SignedHttpRequestAuthenticationOptions() : base(SignedHttpRequestDefaults.AuthenticationScheme) { }

        public IRequestSignatureVerifier RequestSignatureVerifier { get; set; }
        public string Scheme { get; set; } = SignedHttpRequestDefaults.AuthenticationScheme;
        public string Realm { get; set; }

        public Func<IOwinRequest, RequestSignatureVerificationResultSuccess, Task> OnIdentityVerified { get; set; }
        public Func<IOwinRequest, RequestSignatureVerificationResultFailure, Task> OnIdentityVerificationFailed { get; set; }

        internal void Validate() {
            if (string.IsNullOrEmpty(Scheme)) throw new ValidationException($"The {nameof(SignedHttpRequestAuthenticationOptions)} do not specify a valid {nameof(Scheme)}.");
            if (string.IsNullOrEmpty(Realm)) throw new ValidationException($"The {nameof(SignedHttpRequestAuthenticationOptions)} do not specify a valid {nameof(Realm)}.");
            if (RequestSignatureVerifier == null) throw new ValidationException($"The {nameof(SignedHttpRequestAuthenticationOptions)} do not specify a valid {nameof(RequestSignatureVerifier)}.");
        }
    }
}