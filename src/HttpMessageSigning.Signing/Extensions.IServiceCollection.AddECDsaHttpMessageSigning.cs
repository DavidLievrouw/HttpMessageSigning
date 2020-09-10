using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;

namespace Dalion.HttpMessageSigning.Signing {
    public static partial class Extensions {
        /// <summary>
        ///     Adds http message signing registrations to the specified
        ///     <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="keyId">
        ///     The <see cref="T:Dalion.HttpMessageSigning.KeyId" /> that the server can use to identify the client
        ///     application.
        /// </param>
        /// <param name="services">
        ///     The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to add the
        ///     registrations to.
        /// </param>
        /// <param name="ecdsa">The ECDsa key pair.</param>
        /// <returns>
        ///     The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to which the registrations
        ///     were added.
        /// </returns>
        [ExcludeFromCodeCoverage]
        public static IServiceCollection AddECDsaHttpMessageSigning(this IServiceCollection services, KeyId keyId, ECDsa ecdsa) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (ecdsa == null) throw new ArgumentNullException(nameof(ecdsa));

            return services.AddECDsaHttpMessageSigning(keyId, ecdsa, settings => {});
        }

        /// <summary>
        ///     Adds http message signing registrations to the specified
        ///     <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="keyId">
        ///     The <see cref="T:Dalion.HttpMessageSigning.KeyId" /> that the server can use to identify the client
        ///     application.
        /// </param>
        /// <param name="services">
        ///     The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to add the
        ///     registrations to.
        /// </param>
        /// <param name="ecdsa">The ECDsa key pair.</param>
        /// <param name="signingSettingsConfig">The action that configures the signing settings.</param>
        /// <returns>
        ///     The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to which the registrations
        ///     were added.
        /// </returns>
        [ExcludeFromCodeCoverage]
        public static IServiceCollection AddECDsaHttpMessageSigning(this IServiceCollection services, KeyId keyId, ECDsa ecdsa, Action<SigningSettings> signingSettingsConfig) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (ecdsa == null) throw new ArgumentNullException(nameof(ecdsa));

            return services.AddECDsaHttpMessageSigning(
                prov => keyId,
                prov => ecdsa,
                (prov, settings) => signingSettingsConfig(settings)
            );
        }
                
        /// <summary>
        ///     Adds http message signing registrations to the specified
        ///     <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="keyIdFactory">
        ///     The factory that creates the <see cref="T:Dalion.HttpMessageSigning.KeyId" /> that the server can use to identify the client
        ///     application.
        /// </param>
        /// <param name="services">
        ///     The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to add the
        ///     registrations to.
        /// </param>
        /// <param name="ecdsaFactory">The factory that creates the ECDsa key pair.</param>
        /// <param name="signingSettingsConfig">The action that configures the signing settings.</param>
        /// <returns>
        ///     The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to which the registrations
        ///     were added.
        /// </returns>
        [ExcludeFromCodeCoverage]
        public static IServiceCollection AddECDsaHttpMessageSigning(this IServiceCollection services, Func<IServiceProvider, KeyId> keyIdFactory, Func<IServiceProvider, ECDsa> ecdsaFactory, Action<SigningSettings> signingSettingsConfig) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (keyIdFactory == null) throw new ArgumentNullException(nameof(keyIdFactory));
            if (ecdsaFactory == null) throw new ArgumentNullException(nameof(ecdsaFactory));

            return services.AddECDsaHttpMessageSigning(
                keyIdFactory,
                ecdsaFactory,
                (prov, settings) => signingSettingsConfig(settings)
            );
        }
        
        /// <summary>
        ///     Adds http message signing registrations to the specified
        ///     <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="keyIdFactory">
        ///     The factory that creates the <see cref="T:Dalion.HttpMessageSigning.KeyId" /> that the server can use to identify the client
        ///     application.
        /// </param>
        /// <param name="services">
        ///     The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to add the
        ///     registrations to.
        /// </param>
        /// <param name="ecdsaFactory">The factory that creates the ECDsa key pair.</param>
        /// <param name="signingSettingsConfig">The action that configures the signing settings.</param>
        /// <returns>
        ///     The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to which the registrations
        ///     were added.
        /// </returns>
        [ExcludeFromCodeCoverage]
        public static IServiceCollection AddECDsaHttpMessageSigning(this IServiceCollection services, Func<IServiceProvider, KeyId> keyIdFactory, Func<IServiceProvider, ECDsa> ecdsaFactory, Action<IServiceProvider, SigningSettings> signingSettingsConfig) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (keyIdFactory == null) throw new ArgumentNullException(nameof(keyIdFactory));
            if (ecdsaFactory == null) throw new ArgumentNullException(nameof(ecdsaFactory));

            return services.AddHttpMessageSigning(
                keyIdFactory,
                prov => {
                    var signingSettings = new SigningSettings {
                        SignatureAlgorithm = new ECDsaSignatureAlgorithm(HashAlgorithmName.SHA512, ecdsaFactory(prov))
                    };
                    signingSettingsConfig?.Invoke(prov, signingSettings);
                    return signingSettings;
                });
        }
    }
}