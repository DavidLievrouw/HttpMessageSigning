using System;
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
        /// <param name="rsa">The RSA key pair.</param>
        /// <returns>
        ///     The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to which the registrations
        ///     were added.
        /// </returns>
        public static IServiceCollection AddRSAHttpMessageSigning(this IServiceCollection services, KeyId keyId, RSA rsa) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (rsa == null) throw new ArgumentNullException(nameof(rsa));

            return services.AddRSAHttpMessageSigning(keyId, rsa, settings => {});
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
        /// <param name="rsa">The RSA key pair.</param>
        /// <param name="signingSettingsConfig">The action that configures the signing settings.</param>
        /// <returns>
        ///     The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to which the registrations
        ///     were added.
        /// </returns>
        public static IServiceCollection AddRSAHttpMessageSigning(this IServiceCollection services, KeyId keyId, RSA rsa, Action<SigningSettings> signingSettingsConfig) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (rsa == null) throw new ArgumentNullException(nameof(rsa));

            return services.AddHttpMessageSigning(keyId, prov => {
                var signingSettings = new SigningSettings {
                    SignatureAlgorithm = new RSASignatureAlgorithm(HashAlgorithmName.SHA256, rsa)
                };
                signingSettingsConfig?.Invoke(signingSettings);
                return signingSettings;
            });
        }
    }
}