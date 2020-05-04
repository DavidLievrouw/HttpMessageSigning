using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        /// <remarks>This overload assumes that you registered an <see cref="IClientStore" />.</remarks>
        [ExcludeFromCodeCoverage]
        public static IServiceCollection AddHttpMessageSignatureVerification(this IServiceCollection services) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            
            return services
                .AddHttpMessageSignatureVerifier()
                .AddSingleton<ISignatureParser>(prov => new SignatureParser(prov.GetService<ILogger<SignatureParser>>()))
                .AddSingleton<IRequestSignatureVerifier, RequestSignatureVerifier>();
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
        [ExcludeFromCodeCoverage]
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
        [ExcludeFromCodeCoverage]
        public static IServiceCollection AddHttpMessageSignatureVerification(this IServiceCollection services, Func<IServiceProvider, IEnumerable<Client>> allowedClientsFactory) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (allowedClientsFactory == null) throw new ArgumentNullException(nameof(allowedClientsFactory));

            return services.AddHttpMessageSignatureVerification(prov => {
                var store = new InMemoryClientStore();
                var allowedClients = allowedClientsFactory(prov);
                foreach (var client in allowedClients) {
                    store.Register(client).ConfigureAwait(false).GetAwaiter().GetResult();
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
        [ExcludeFromCodeCoverage]
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
                .AddHttpMessageSignatureVerifier()
                .AddSingleton<ISignatureParser>(prov => new SignatureParser(prov.GetService<ILogger<SignatureParser>>()))
                .AddSingleton(clientStoreFactory)
                .AddSingleton<IRequestSignatureVerifier, RequestSignatureVerifier>();
        }
    }
}