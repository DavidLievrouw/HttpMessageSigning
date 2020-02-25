using System;
using Dalion.HttpMessageSigning.Logging;
using Dalion.HttpMessageSigning.Signing;
using Dalion.HttpMessageSigning.SigningString;
using Microsoft.Extensions.DependencyInjection;

namespace Dalion.HttpMessageSigning {
    public static class Extensions {
        /// <summary>
        ///     Adds http message signing registrations to the specified
        ///     <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="services">
        ///     The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to add the
        ///     registrations to.
        /// </param>
        /// <param name="clientKey">The client key.</param>
        /// <returns>
        ///     The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to which the registrations
        ///     were added.
        /// </returns>
        public static IServiceCollection AddHttpMessageSigning(this IServiceCollection services, ClientKey clientKey) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (clientKey == null) throw new ArgumentNullException(nameof(clientKey));

            return services.AddHttpMessageSigning(prov => new SigningSettings {
                ClientKey = clientKey
            });
        }

        /// <summary>
        ///     Adds http message signing registrations to the specified
        ///     <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="services">
        ///     The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to add the
        ///     registrations to.
        /// </param>
        /// <param name="clientKeyConfig">The action that configures the client key.</param>
        /// <returns>
        ///     The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to which the registrations
        ///     were added.
        /// </returns>
        public static IServiceCollection AddHttpMessageSigning(this IServiceCollection services, Action<ClientKey> clientKeyConfig) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (clientKeyConfig == null) throw new ArgumentNullException(nameof(clientKeyConfig));

            return services.AddHttpMessageSigning(prov => {
                var clientKey = new ClientKey();
                clientKeyConfig.Invoke(clientKey);
                return new SigningSettings {
                    ClientKey = clientKey
                };
            });
        }

        /// <summary>
        ///     Adds http message signing registrations to the specified
        ///     <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="services">
        ///     The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to add the
        ///     registrations to.
        /// </param>
        /// <param name="clientKeyFactory">The factory that creates the client key.</param>
        /// <returns>
        ///     The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to which the registrations
        ///     were added.
        /// </returns>
        public static IServiceCollection AddHttpMessageSigning(this IServiceCollection services, Func<IServiceProvider, ClientKey> clientKeyFactory) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (clientKeyFactory == null) throw new ArgumentNullException(nameof(clientKeyFactory));

            return services.AddHttpMessageSigning(prov => new SigningSettings {
                ClientKey = clientKeyFactory.Invoke(prov)
            });
        }

        /// <summary>
        ///     Adds http message signing registrations to the specified
        ///     <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="services">
        ///     The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to add the
        ///     registrations to.
        /// </param>
        /// <param name="signingSettings">The signing settings.</param>
        /// <returns>
        ///     The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to which the registrations
        ///     were added.
        /// </returns>
        public static IServiceCollection AddHttpMessageSigning(this IServiceCollection services, SigningSettings signingSettings) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (signingSettings == null) throw new ArgumentNullException(nameof(signingSettings));

            return services.AddHttpMessageSigning(prov => signingSettings);
        }

        /// <summary>
        ///     Adds http message signing registrations to the specified
        ///     <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="services">
        ///     The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to add the
        ///     registrations to.
        /// </param>
        /// <param name="signingSettingsConfig">The action that configures the signing settings.</param>
        /// <returns>
        ///     The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to which the registrations
        ///     were added.
        /// </returns>
        public static IServiceCollection AddHttpMessageSigning(this IServiceCollection services, Action<SigningSettings> signingSettingsConfig) {
            if (services == null) throw new ArgumentNullException(nameof(services));

            return services.AddHttpMessageSigning(prov => {
                var newSettings = new SigningSettings();
                signingSettingsConfig?.Invoke(newSettings);
                return newSettings;
            });
        }

        /// <summary>
        ///     Adds http message signing registrations to the specified
        ///     <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="services">
        ///     The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to add the
        ///     registrations to.
        /// </param>
        /// <param name="signingSettingsFactory">The factory that creates the signing settings.</param>
        /// <returns>
        ///     The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to which the registrations
        ///     were added.
        /// </returns>
        public static IServiceCollection AddHttpMessageSigning(this IServiceCollection services, Func<IServiceProvider, SigningSettings> signingSettingsFactory) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (signingSettingsFactory == null) throw new ArgumentNullException(nameof(signingSettingsFactory));

            return services
                .AddSingleton(typeof(IHttpMessageSigningLogger<>), typeof(NetCoreHttpMessageSigningLogger<>))
                .AddSingleton<ISystemClock, RealSystemClock>()
                .AddSingleton<IBase64Converter, Base64Converter>()
                .AddSingleton<IHashAlgorithmFactory, HashAlgorithmFactory>()
                .AddSingleton<IKeyedHashAlgorithmFactory, KeyedHashAlgorithmFactory>()
                .AddSingleton<IAdditionalSignatureHeadersSetter>(prov => new AdditionalSignatureHeadersSetter(
                    new DateSignatureHeaderEnsurer(), 
                    new DigestSignatureHeaderEnsurer()))
                .AddSingleton<ISignatureCreator, SignatureCreator>()
                .AddSingleton<IAuthorizationHeaderParamCreator, AuthorizationHeaderParamCreator>()
                .AddSingleton<IHeaderAppenderFactory, HeaderAppenderFactory>()
                .AddSingleton<ISigningStringComposer, SigningStringComposer>()
                .AddSingleton<IRequestSignerFactory, RequestSignerFactory>()
                .AddSingleton(prov => {
                    var factory = prov.GetRequiredService<IRequestSignerFactory>();
                    return factory.Create(signingSettingsFactory(prov));
                });
        }
    }
}