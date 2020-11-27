using System;
using Dalion.HttpMessageSigning.SigningString;
using Dalion.HttpMessageSigning.SigningString.RequestTarget;
using Dalion.HttpMessageSigning.Verification.VerificationTasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dalion.HttpMessageSigning.Verification {
    /// <summary>
    ///     Extension methods for this library.
    /// </summary>
    public static partial class Extensions {
        /// <summary>Adds <see cref="ISignatureVerifier"/> registrations to the specified <see cref="IServiceCollection" />.</summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add the registrations to.</param>
        /// <returns>The <see cref="IHttpMessageSigningVerificationBuilder" /> that can be used to configure the verification settings.</returns>
        public static IHttpMessageSigningVerificationBuilder AddHttpMessageSignatureVerification(this IServiceCollection services) {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services = services
                .AddMemoryCache()
                .AddSingleton<IBase64Converter, Base64Converter>()
                .AddSingleton<INonceStore, InMemoryNonceStore>()
                .AddSingleton<INonceAppender, NonceAppender>()
                .AddSingleton<ISystemClock, RealSystemClock>()
                .AddSingleton<IClaimsPrincipalFactory>(new ClaimsPrincipalFactory(typeof(ISignatureVerifier).Assembly.GetName().Version.ToString(2)))
                .AddSingleton<IDefaultSignatureHeadersProvider, DefaultSignatureHeadersProvider>()
                .AddSingleton<ISignatureSanitizer, SignatureSanitizer>()
                .AddSingleton<IRequestTargetEscaper>(provider => new CompositeRequestTargetEscaper(
                    new RFC3986RequestTargetEscaper(), 
                    new RFC2396RequestTargetEscaper(), 
                    new UnescapedRequestTargetEscaper(), 
                    new OriginalStringRequestTargetEscaper()))
                .AddSingleton<IHeaderAppenderFactory, HeaderAppenderFactory>()
                .AddSingleton<ISigningStringComposer, SigningStringComposer>()
                .AddSingleton<IVerificationResultCreatorFactory, VerificationResultCreatorFactory>()
                .AddSingleton<ISigningStringCompositionRequestFactory, SigningStringCompositionRequestFactory>()
                .AddSingleton<ISignatureVerifier>(provider => new SignatureVerifier(
                    provider.GetRequiredService<ISignatureSanitizer>(),
                    new KnownAlgorithmVerificationTask(
                        provider.GetService<ILogger<KnownAlgorithmVerificationTask>>()),
                    new MatchingAlgorithmVerificationTask(
                        provider.GetService<ILogger<MatchingAlgorithmVerificationTask>>()),
                    new CreatedHeaderGuardVerificationTask(),
                    new ExpiresHeaderGuardVerificationTask(),
                    new AllHeadersPresentVerificationTask(),
                    new CreationTimeVerificationTask(
                        provider.GetRequiredService<ISystemClock>()),
                    new ExpirationTimeVerificationTask(
                        provider.GetRequiredService<ISystemClock>()),
                    new NonceVerificationTask(
                        provider.GetRequiredService<INonceStore>(),
                        provider.GetRequiredService<ISystemClock>()), 
                    new DigestVerificationTask(
                        provider.GetRequiredService<IBase64Converter>(),
                        provider.GetService<ILogger<DigestVerificationTask>>()),
                    new MatchingSignatureStringVerificationTask(
                        provider.GetRequiredService<ISigningStringComposer>(),
                        provider.GetRequiredService<IBase64Converter>(),
                        provider.GetRequiredService<ISigningStringCompositionRequestFactory>(),
                        provider.GetService<ILogger<MatchingSignatureStringVerificationTask>>())))
                .AddSingleton<IRequestSignatureVerificationOrchestrator, RequestSignatureVerificationOrchestrator>();
            
            return new HttpMessageSigningVerificationBuilder(services); 
        }
    }
}