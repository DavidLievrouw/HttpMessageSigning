using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dalion.HttpMessageSigning.Verification.AspNetCore {
    public static partial class Extensions {
        /// <summary>Adds support for the ASP.NET Core authentication scheme that verifies HTTP message signatures.</summary>
        /// <param name="builder">The <see cref="IHttpMessageSigningVerificationBuilder" /> that is used to configure verification.</param>
        /// <returns>The <see cref="IHttpMessageSigningVerificationBuilder" /> that can be used to continue configuring the verification settings.</returns>
        [ExcludeFromCodeCoverage]
        public static IHttpMessageSigningVerificationBuilder UseAspNetCoreSignatureVerification(this IHttpMessageSigningVerificationBuilder builder) {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.Services
                .AddSingleton<IAuthenticationHeaderExtractor, DefaultAuthenticationHeaderExtractor>()
                .AddSingleton<ISignatureParser>(prov => new DefaultSignatureParser(
                    prov.GetRequiredService<IAuthenticationHeaderExtractor>(),
                    prov.GetService<ILogger<DefaultSignatureParser>>()))
                .AddSingleton<IRequestSignatureVerifier, RequestSignatureVerifier>();

            return builder;
        }

        /// <summary>Configures HTTP message signature verification to use the specified <see cref="ISignatureParser" />.</summary>
        /// <param name="builder">The <see cref="IHttpMessageSigningVerificationBuilder" /> that is used to configure verification.</param>
        /// <typeparam name="TSignatureParser">The type of the <see cref="ISignatureParser" /> that is to be used.</typeparam>
        /// <returns>The <see cref="IHttpMessageSigningVerificationBuilder" /> that can be used to continue configuring the verification settings.</returns>
        [ExcludeFromCodeCoverage]
        public static IHttpMessageSigningVerificationBuilder UseSignatureParser<TSignatureParser>(this IHttpMessageSigningVerificationBuilder builder)
            where TSignatureParser : ISignatureParser {
            return builder.UseSignatureParser(provider => provider.GetRequiredService<TSignatureParser>());
        }

        /// <summary>Configures HTTP message signature verification to use the specified <see cref="ISignatureParser" />.</summary>
        /// <param name="builder">The <see cref="IHttpMessageSigningVerificationBuilder" /> that is used to configure verification.</param>
        /// <param name="signatureParser">The <see cref="ISignatureParser" /> that is to be used.</param>
        /// <returns>The <see cref="IHttpMessageSigningVerificationBuilder" /> that can be used to continue configuring the verification settings.</returns>
        [ExcludeFromCodeCoverage]
        public static IHttpMessageSigningVerificationBuilder UseSignatureParser(this IHttpMessageSigningVerificationBuilder builder, ISignatureParser signatureParser) {
            if (signatureParser == null) throw new ArgumentNullException(nameof(signatureParser));

            return builder.UseSignatureParser(provider => signatureParser);
        }

        /// <summary>Configures HTTP message signature verification to use the specified <see cref="ISignatureParser" />.</summary>
        /// <param name="builder">The <see cref="IHttpMessageSigningVerificationBuilder" /> that is used to configure verification.</param>
        /// <param name="signatureParserFactory">The factory that creates the <see cref="ISignatureParser" /> that is to be used.</param>
        /// <returns>The <see cref="IHttpMessageSigningVerificationBuilder" /> that can be used to continue configuring the verification settings.</returns>
        [ExcludeFromCodeCoverage]
        public static IHttpMessageSigningVerificationBuilder UseSignatureParser(
            this IHttpMessageSigningVerificationBuilder builder,
            Func<IServiceProvider, ISignatureParser> signatureParserFactory) {
            if (signatureParserFactory == null) throw new ArgumentNullException(nameof(signatureParserFactory));

            builder.Services.AddSingleton(signatureParserFactory);

            return builder;
        }

        /// <summary>Configures HTTP message signature verification to use the specified <see cref="IAuthenticationHeaderExtractor" />.</summary>
        /// <param name="builder">The <see cref="IHttpMessageSigningVerificationBuilder" /> that is used to configure verification.</param>
        /// <typeparam name="TAuthenticationHeaderExtractor">The type of the <see cref="IAuthenticationHeaderExtractor" /> that is to be used.</typeparam>
        /// <returns>The <see cref="IHttpMessageSigningVerificationBuilder" /> that can be used to continue configuring the verification settings.</returns>
        [ExcludeFromCodeCoverage]
        public static IHttpMessageSigningVerificationBuilder UseAuthenticationHeaderExtractor<TAuthenticationHeaderExtractor>(this IHttpMessageSigningVerificationBuilder builder)
            where TAuthenticationHeaderExtractor : IAuthenticationHeaderExtractor {
            return builder.UseAuthenticationHeaderExtractor(provider => provider.GetRequiredService<TAuthenticationHeaderExtractor>());
        }

        /// <summary>Configures HTTP message signature verification to use the specified <see cref="IAuthenticationHeaderExtractor" />.</summary>
        /// <param name="builder">The <see cref="IHttpMessageSigningVerificationBuilder" /> that is used to configure verification.</param>
        /// <param name="authenticationHeaderExtractor">The <see cref="IAuthenticationHeaderExtractor" /> that is to be used.</param>
        /// <returns>The <see cref="IHttpMessageSigningVerificationBuilder" /> that can be used to continue configuring the verification settings.</returns>
        [ExcludeFromCodeCoverage]
        public static IHttpMessageSigningVerificationBuilder UseAuthenticationHeaderExtractor(
            this IHttpMessageSigningVerificationBuilder builder,
            IAuthenticationHeaderExtractor authenticationHeaderExtractor) {
            if (authenticationHeaderExtractor == null) throw new ArgumentNullException(nameof(authenticationHeaderExtractor));

            return builder.UseAuthenticationHeaderExtractor(provider => authenticationHeaderExtractor);
        }

        /// <summary>Configures HTTP message signature verification to use the specified <see cref="IAuthenticationHeaderExtractor" />.</summary>
        /// <param name="builder">The <see cref="IHttpMessageSigningVerificationBuilder" /> that is used to configure verification.</param>
        /// <param name="authenticationHeaderExtractorFactory">The factory that creates the <see cref="IAuthenticationHeaderExtractor" /> that is to be used.</param>
        /// <returns>The <see cref="IHttpMessageSigningVerificationBuilder" /> that can be used to continue configuring the verification settings.</returns>
        [ExcludeFromCodeCoverage]
        public static IHttpMessageSigningVerificationBuilder UseAuthenticationHeaderExtractor(
            this IHttpMessageSigningVerificationBuilder builder,
            Func<IServiceProvider, IAuthenticationHeaderExtractor> authenticationHeaderExtractorFactory) {
            if (authenticationHeaderExtractorFactory == null) throw new ArgumentNullException(nameof(authenticationHeaderExtractorFactory));

            builder.Services.AddSingleton(authenticationHeaderExtractorFactory);

            return builder;
        }
    }
}