using Microsoft.AspNetCore.Authentication;

namespace Dalion.HttpMessageSigning.Verification.AspNetCore {
    public class SignedRequestAuthenticationOptions : AuthenticationSchemeOptions {
        public string Realm { get; set; }
    }
}