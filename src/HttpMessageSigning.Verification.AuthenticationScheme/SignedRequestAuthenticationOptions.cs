using Microsoft.AspNetCore.Authentication;

namespace Dalion.HttpMessageSigning.Verification.AuthenticationScheme {
    public class SignedRequestAuthenticationOptions : AuthenticationSchemeOptions {
        public string Realm { get; set; }
    }
}