using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dalion.HttpMessageSigning.Verification.Owin {
    public static partial class Extensions {
        /// <summary>Adds support for the Owin authentication middleware that verifies HTTP message signatures.</summary>
        /// <param name="builder">The <see cref="IHttpMessageSigningVerificationBuilder" /> that is used to configure verification.</param>
        /// <returns>The <see cref="IHttpMessageSigningVerificationBuilder" /> that can be used to continue configuring the verification settings.</returns>
        [ExcludeFromCodeCoverage]
        public static IHttpMessageSigningVerificationBuilder UseOwinSignatureVerification(this IHttpMessageSigningVerificationBuilder builder) {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.Services
                .AddSingleton<ISignatureParser>(prov => new SignatureParser(prov.GetService<ILogger<SignatureParser>>()))
                .AddSingleton<IRequestSignatureVerifier, RequestSignatureVerifier>();

            return builder;
        }
    }
}