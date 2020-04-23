using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Dalion.HttpMessageSigning.Verification.AspNetCore {
    public class SignedRequestAuthenticationOptions : AuthenticationSchemeOptions {
        public string Realm { get; set; }
        public string Scheme { get; set; } = SignedHttpRequestDefaults.AuthenticationScheme;
        public Func<HttpRequest, RequestSignatureVerificationResultSuccess, Task> OnIdentityVerified { get; set; }
        public Func<HttpRequest, RequestSignatureVerificationResultFailure, Task> OnIdentityVerificationFailed { get; set; }
    }
}