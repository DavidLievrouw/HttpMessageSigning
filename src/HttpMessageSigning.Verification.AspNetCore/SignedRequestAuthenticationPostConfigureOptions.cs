using System;
using Microsoft.Extensions.Options;

namespace Dalion.HttpMessageSigning.Verification.AspNetCore {
    internal class SignedRequestAuthenticationPostConfigureOptions : IPostConfigureOptions<SignedRequestAuthenticationOptions> {
        public void PostConfigure(string name, SignedRequestAuthenticationOptions options) {
            if (string.IsNullOrEmpty(options.Realm)) throw new ValidationException($"{nameof(options.Realm)} must be provided in {nameof(options)}.");
            if (string.IsNullOrEmpty(options.Scheme)) throw new ValidationException($"{nameof(options.Scheme)} must be provided in {nameof(options)}.");
            if (options.Scheme.Contains(' ', StringComparison.Ordinal)) throw new ValidationException($"{nameof(options.Scheme)} cannot contain whitespace.");
        }
    }
}