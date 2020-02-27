using System;
using Dalion.HttpMessageSigning.Logging;
using Dalion.HttpMessageSigning.SigningString;
using Dalion.HttpMessageSigning.Verification.VerificationTasks;
using Microsoft.Extensions.DependencyInjection;

namespace Dalion.HttpMessageSigning.Verification {
    public static partial class Extensions {
        /// <summary>
        ///     Adds http message signature verification registrations to the specified
        ///     <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="services">
        ///     The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to add the
        ///     registrations to.
        /// </param>
        /// <param name="clientStore">The store that contains the registered clients.</param>
        /// <returns>
        ///     The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to which the registrations
        ///     were added.
        /// </returns>
        public static IServiceCollection AddHttpMessageSignatureVerification(this IServiceCollection services, IClientStore clientStore) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (clientStore == null) throw new ArgumentNullException(nameof(clientStore));

            return services.AddHttpMessageSignatureVerification(prov => clientStore);
        }

        /// <summary>
        ///     Adds http message signature verification registrations to the specified
        ///     <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="services">
        ///     The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to add the
        ///     registrations to.
        /// </param>
        /// <param name="clientStoreFactory">The factory that creates the store that contains the registered clients.</param>
        /// <returns>
        ///     The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to which the registrations
        ///     were added.
        /// </returns>
        public static IServiceCollection AddHttpMessageSignatureVerification(this IServiceCollection services, Func<IServiceProvider, IClientStore> clientStoreFactory) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (clientStoreFactory == null) throw new ArgumentNullException(nameof(clientStoreFactory));

            return services
                .AddSingleton(typeof(IHttpMessageSigningLogger<>), typeof(NetCoreHttpMessageSigningLogger<>))
                .AddSingleton<IBase64Converter, Base64Converter>()
                .AddSingleton<ISignatureParser, SignatureParser>()
                .AddSingleton<IClaimsPrincipalFactory, ClaimsPrincipalFactory>()
                .AddSingleton<IDefaultSignatureHeadersProvider, DefaultSignatureHeadersProvider>()
                .AddSingleton<ISignatureSanitizer, SignatureSanitizer>()
                .AddSingleton(clientStoreFactory)
                .AddSingleton<IHeaderAppenderFactory, HeaderAppenderFactory>()
                .AddSingleton<ISigningStringComposer, SigningStringComposer>()
                .AddSingleton<ISignatureVerifier>(provider => new SignatureVerifier(
                    new KnownAlgorithmVerificationTask(), 
                    new MatchingAlgorithmVerificationTask(), 
                    new CreatedHeaderGuardVerificationTask(), 
                    new ExpiresHeaderGuardVerificationTask(), 
                    new AllHeadersPresentVerificationTask(), 
                    new CreationTimeVerificationTask(
                        provider.GetRequiredService<ISystemClock>()), 
                    new ExpirationTimeVerificationTask(
                        provider.GetRequiredService<ISystemClock>()), 
                    new DigestVerificationTask(), 
                    new MatchingSignatureVerificationTask(
                        provider.GetRequiredService<ISigningStringComposer>(),
                        provider.GetRequiredService<IBase64Converter>())))
                .AddSingleton<IRequestSignatureVerifier, RequestSignatureVerifier>();
        }
    }
}