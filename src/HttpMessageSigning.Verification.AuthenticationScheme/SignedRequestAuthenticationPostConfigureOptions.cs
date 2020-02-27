using System;
using Microsoft.Extensions.Options;

namespace Dalion.HttpMessageSigning.Verification.AuthenticationScheme {
    internal class SignedRequestAuthenticationPostConfigureOptions : IPostConfigureOptions<SignedRequestAuthenticationOptions> {
        public void PostConfigure(string name, SignedRequestAuthenticationOptions options) {
            if (string.IsNullOrEmpty(options.Realm)) throw new InvalidOperationException($"{nameof(options.Realm)} must be provided in {nameof(options)}.");
        }
    }
}