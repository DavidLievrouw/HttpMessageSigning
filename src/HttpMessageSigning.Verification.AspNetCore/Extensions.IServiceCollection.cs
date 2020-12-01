using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dalion.HttpMessageSigning.Verification.AspNetCore {
    public static partial class Extensions {
        /// <summary>Adds http message signature verification and ASP.NET Core authentication scheme registrations to the specified <see cref="IServiceCollection" />.</summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add the registrations to.</param>
        /// <returns>The <see cref="IHttpMessageSigningVerificationBuilder" /> that can be used to configure the verification settings.</returns>
        [Obsolete("Please use the '" + nameof(Verification.Extensions.AddHttpMessageSignatureVerification) + "' and '" + nameof(UseAspNetCoreSignatureVerification) + "' methods instead.")]
        [ExcludeFromCodeCoverage]
        public static IHttpMessageSigningVerificationBuilder AddHttpMessageSignatureVerificationForAspNetCore(this IServiceCollection services) {
            if (services == null) throw new ArgumentNullException(nameof(services));

            return services
                .AddHttpMessageSignatureVerification()
                .UseAspNetCoreSignatureVerification();
        }
        
        /// <summary>Adds http message signature verification registrations to the specified <see cref="IServiceCollection" />.</summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add the registrations to.</param>
        /// <param name="firstClient">The client that is allowed to authenticate.</param>
        /// <param name="additionalClients">The clients that are allowed to authenticate.</param>
        /// <returns>The <see cref="IServiceCollection" /> to which the registrations were added.</returns>
        [Obsolete("Please use the '" + nameof(UseAspNetCoreSignatureVerification) + "' and '" + nameof(IHttpMessageSigningVerificationBuilder.UseClient) + "' methods of the '" + nameof(IHttpMessageSigningVerificationBuilder) + "' instead.")]
        [ExcludeFromCodeCoverage]
        public static IServiceCollection AddHttpMessageSignatureVerification(this IServiceCollection services, Client firstClient, params Client[] additionalClients) {
            if (services == null) throw new ArgumentNullException(nameof(services));

            var firstClientArray = firstClient == null ? Array.Empty<Client>() : new[] {firstClient};
            var allClients = firstClientArray.Concat(additionalClients ?? Array.Empty<Client>());
            
            return services.AddHttpMessageSignatureVerification(prov => allClients);
        }

        /// <summary>Adds http message signature verification registrations to the specified <see cref="IServiceCollection" />.</summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add the registrations to.</param>
        /// <param name="allowedClientsFactory">The factory that creates the clients that are allowed to authenticate.</param>
        /// <returns>The <see cref="IServiceCollection" /> to which the registrations were added.</returns>
        [Obsolete("Please use the '" + nameof(UseAspNetCoreSignatureVerification) + "' and '" + nameof(IHttpMessageSigningVerificationBuilder.UseClient) + "' method of the '" + nameof(IHttpMessageSigningVerificationBuilder) + "' instead.")]
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

        /// <summary>Adds http message signature verification registrations to the specified <see cref="IServiceCollection" />.</summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add the registrations to.</param>
        /// <param name="clientStore">The store that contains the registered clients.</param>
        /// <returns>The <see cref="IServiceCollection" /> to which the registrations were added.</returns>
        [Obsolete("Please use the '" + nameof(UseAspNetCoreSignatureVerification) + "' and '" + nameof(IHttpMessageSigningVerificationBuilder.UseClientStore) + "' method of the '" + nameof(IHttpMessageSigningVerificationBuilder) + "' instead.")]
        [ExcludeFromCodeCoverage]
        public static IServiceCollection AddHttpMessageSignatureVerification(this IServiceCollection services, IClientStore clientStore) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (clientStore == null) throw new ArgumentNullException(nameof(clientStore));

            return services.AddHttpMessageSignatureVerification(prov => clientStore);
        }

        /// <summary>Adds http message signature verification registrations to the specified <see cref="IServiceCollection" />.</summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add the registrations to.</param>
        /// <param name="clientStoreFactory">The factory that creates the store that contains the registered clients.</param>
        /// <returns>The <see cref="IServiceCollection" /> to which the registrations were added.</returns>
        [Obsolete("Please use the '" + nameof(UseAspNetCoreSignatureVerification) + "' and '" + nameof(IHttpMessageSigningVerificationBuilder.UseClientStore) + "' method of the '" + nameof(IHttpMessageSigningVerificationBuilder) + "' instead.")]
        public static IServiceCollection AddHttpMessageSignatureVerification(this IServiceCollection services, Func<IServiceProvider, IClientStore> clientStoreFactory) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (clientStoreFactory == null) throw new ArgumentNullException(nameof(clientStoreFactory));

            return services
                .AddHttpMessageSignatureVerification().Services
                .AddSingleton<ISignatureParser>(prov => new SignatureParser(prov.GetService<ILogger<SignatureParser>>()))
                .AddSingleton(clientStoreFactory)
                .AddSingleton<IRequestSignatureVerifier, RequestSignatureVerifier>();
        }
    }
}