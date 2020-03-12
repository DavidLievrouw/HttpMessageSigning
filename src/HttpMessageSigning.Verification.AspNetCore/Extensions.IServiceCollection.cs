using System;
using System.Collections.Generic;
using Dalion.HttpMessageSigning.SigningString;
using Dalion.HttpMessageSigning.Verification.VerificationTasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dalion.HttpMessageSigning.Verification.AspNetCore {
    public static partial class Extensions {
        /// <summary>
        ///     Adds http message signature verification registrations to the specified
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
        public static IServiceCollection AddHttpMessageSignatureVerification(this IServiceCollection services) {
            if (services == null) throw new ArgumentNullException(nameof(services));

            return services.AddHttpMessageSignatureVerification(prov => new InMemoryClientStore());
        }
        
        /// <summary>
        ///     Adds http message signature verification registrations to the specified
        ///     <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="services">
        ///     The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to add the
        ///     registrations to.
        /// </param>
        /// <param name="allowedClients">The clients that are allowed to authenticate.</param>
        /// <returns>
        ///     The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to which the registrations
        ///     were added.
        /// </returns>
        public static IServiceCollection AddHttpMessageSignatureVerification(this IServiceCollection services, params Client[] allowedClients) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (allowedClients == null) allowedClients = Array.Empty<Client>();

            return services.AddHttpMessageSignatureVerification(prov => allowedClients);
        }
                
        /// <summary>
        ///     Adds http message signature verification registrations to the specified
        ///     <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="services">
        ///     The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to add the
        ///     registrations to.
        /// </param>
        /// <param name="allowedClientsFactory">The factory that creates the clients that are allowed to authenticate.</param>
        /// <returns>
        ///     The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to which the registrations
        ///     were added.
        /// </returns>
        public static IServiceCollection AddHttpMessageSignatureVerification(this IServiceCollection services, Func<IServiceProvider, IEnumerable<Client>> allowedClientsFactory) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (allowedClientsFactory == null) throw new ArgumentNullException(nameof(allowedClientsFactory));

            return services.AddHttpMessageSignatureVerification(prov => {
                var store = new InMemoryClientStore();
                var allowedClients = allowedClientsFactory(prov);
                foreach (var client in allowedClients) {
                    store.Register(client).GetAwaiter().GetResult();
                }
                return store;
            });
        }
        
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
                .AddSingleton<IBase64Converter, Base64Converter>()
                .AddSingleton<ISystemClock, RealSystemClock>()
                .AddSingleton<ISignatureParser, SignatureParser>()
                .AddSingleton<IClaimsPrincipalFactory>(new ClaimsPrincipalFactory(typeof(IRequestSignatureVerifier).Assembly.GetName().Version.ToString(2)))
                .AddSingleton<IDefaultSignatureHeadersProvider, DefaultSignatureHeadersProvider>()
                .AddSingleton<ISignatureSanitizer, SignatureSanitizer>()
                .AddSingleton(clientStoreFactory)
                .AddSingleton<IHeaderAppenderFactory, HeaderAppenderFactory>()
                .AddSingleton<ISigningStringComposer, SigningStringComposer>()
                .AddSingleton<ISignatureVerifier>(provider => new SignatureVerifier(
                    new KnownAlgorithmVerificationTask(
                        provider.GetRequiredService<ILogger<KnownAlgorithmVerificationTask>>()), 
                    new MatchingAlgorithmVerificationTask(
                        provider.GetRequiredService<ILogger<MatchingAlgorithmVerificationTask>>()), 
                    new CreatedHeaderGuardVerificationTask(), 
                    new ExpiresHeaderGuardVerificationTask(), 
                    new AllHeadersPresentVerificationTask(), 
                    new CreationTimeVerificationTask(
                        provider.GetRequiredService<ISystemClock>()), 
                    new ExpirationTimeVerificationTask(
                        provider.GetRequiredService<ISystemClock>()), 
                    new DigestVerificationTask(
                        provider.GetRequiredService<IBase64Converter>(),
                        provider.GetRequiredService<ILogger<DigestVerificationTask>>()), 
                    new MatchingSignatureStringVerificationTask(
                        provider.GetRequiredService<ISigningStringComposer>(),
                        provider.GetRequiredService<IBase64Converter>(),
                        provider.GetRequiredService<ILogger<MatchingSignatureStringVerificationTask>>())))
                .AddSingleton<IRequestSignatureVerifier, RequestSignatureVerifier>();
        }
    }
}