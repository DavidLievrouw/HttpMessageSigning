using System;
using Dalion.HttpMessageSigning.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Dalion.HttpMessageSigning.Validation {
    public static partial class Extensions {
        /// <summary>
        ///     Adds http message signature validation registrations to the specified
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
        public static IServiceCollection AddHttpMessageSignatureValidation(this IServiceCollection services, IClientStore clientStore) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (clientStore == null) throw new ArgumentNullException(nameof(clientStore));

            return services.AddHttpMessageSignatureValidation(prov => clientStore);
        }

        /// <summary>
        ///     Adds http message signature validation registrations to the specified
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
        public static IServiceCollection AddHttpMessageSignatureValidation(this IServiceCollection services, Func<IServiceProvider, IClientStore> clientStoreFactory) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (clientStoreFactory == null) throw new ArgumentNullException(nameof(clientStoreFactory));

            return services
                .AddSingleton(typeof(IHttpMessageSigningLogger<>), typeof(NetCoreHttpMessageSigningLogger<>))
                .AddSingleton<ISignatureParser, SignatureParser>()
                .AddSingleton<IRequestSignatureValidator, RequestSignatureValidator>();
        }
    }
}