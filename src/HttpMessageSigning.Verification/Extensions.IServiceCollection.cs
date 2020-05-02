using System;
using Dalion.HttpMessageSigning.SigningString;
using Dalion.HttpMessageSigning.Verification.VerificationTasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dalion.HttpMessageSigning.Verification {
    public static partial class Extensions {
        /// <summary>
        ///     Adds <see cref="ISignatureVerifier"/> registrations to the specified
        ///     <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="services">
        ///     The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to add the
        ///     registrations to.
        /// </param>
        /// <returns>
        ///     The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to which the registrations
        ///     were added.
        /// </returns>
        public static IServiceCollection AddHttpMessageSignatureVerifier(this IServiceCollection services) {
            if (services == null) throw new ArgumentNullException(nameof(services));

            return services
                .AddMemoryCache()
                .AddSingleton<IBase64Converter, Base64Converter>()
                .AddSingleton<INonceStore, InMemoryNonceStore>()
                .AddSingleton<INonceAppender, NonceAppender>()
                .AddSingleton<ISystemClock, RealSystemClock>()
                .AddSingleton<IClaimsPrincipalFactory>(new ClaimsPrincipalFactory(typeof(ISignatureVerifier).Assembly.GetName().Version.ToString(2)))
                .AddSingleton<IDefaultSignatureHeadersProvider, DefaultSignatureHeadersProvider>()
                .AddSingleton<ISignatureSanitizer, SignatureSanitizer>()
                .AddSingleton<IHeaderAppenderFactory, HeaderAppenderFactory>()
                .AddSingleton<ISigningStringComposer, SigningStringComposer>()
                .AddSingleton<IVerificationResultCreatorFactory, VerificationResultCreatorFactory>()
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
                        provider.GetService<ILogger<MatchingSignatureStringVerificationTask>>())));
        }
    }
}