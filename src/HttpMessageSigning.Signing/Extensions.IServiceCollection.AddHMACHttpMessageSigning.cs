using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;

namespace Dalion.HttpMessageSigning.Signing {
    public static partial class Extensions {
        /// <summary>Adds http message signing registrations to the specified <see cref="IServiceCollection" />.</summary>
        /// <param name="keyId">The <see cref="T:Dalion.HttpMessageSigning.KeyId" /> that the server can use to identify the client application.</param>
        /// <param name="services">The <see cref="IServiceCollection" /> to add the registrations to.</param>
        /// <param name="hmacSecret">The HMAC symmetric key.</param>
        /// <returns>The <see cref="IServiceCollection" /> to which the registrations were added.</returns>
        [ExcludeFromCodeCoverage]
        [Obsolete("Please use the '" + nameof(AddHttpMessageSigning) + "' method without parameters, and use a " + nameof(IHttpMessageSigningBuilder) + " to continue configuration.")]
        public static IServiceCollection AddHMACHttpMessageSigning(this IServiceCollection services, KeyId keyId, string hmacSecret) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (hmacSecret == null) throw new ArgumentNullException(nameof(hmacSecret));

            return services.AddHMACHttpMessageSigning(keyId, hmacSecret, settings => {});
        }
        
        /// <summary>Adds http message signing registrations to the specified <see cref="IServiceCollection" />.</summary>
        /// <param name="keyId">The <see cref="T:Dalion.HttpMessageSigning.KeyId" /> that the server can use to identify the client application.</param>
        /// <param name="services">The <see cref="IServiceCollection" /> to add the registrations to.</param>
        /// <param name="hmacSecret">The HMAC symmetric key.</param>
        /// <param name="signingSettingsConfig">The action that configures the signing settings.</param>
        /// <returns>The <see cref="IServiceCollection" /> to which the registrations were added.</returns>
        [ExcludeFromCodeCoverage]
        [Obsolete("Please use the '" + nameof(AddHttpMessageSigning) + "' method without parameters, and use a " + nameof(IHttpMessageSigningBuilder) + " to continue configuration.")]
        public static IServiceCollection AddHMACHttpMessageSigning(this IServiceCollection services, KeyId keyId, string hmacSecret, Action<SigningSettings> signingSettingsConfig) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (hmacSecret == null) throw new ArgumentNullException(nameof(hmacSecret));

            return services.AddHMACHttpMessageSigning(
                prov => keyId,
                prov => hmacSecret,
                (prov, settings) => signingSettingsConfig(settings));
        }
        
        /// <summary>Adds http message signing registrations to the specified <see cref="IServiceCollection" />.</summary>
        /// <param name="keyIdFactory">The factory that creates the <see cref="T:Dalion.HttpMessageSigning.KeyId" /> that the server can use to identify the client application.</param>
        /// <param name="services">The <see cref="IServiceCollection" /> to add the registrations to.</param>
        /// <param name="hmacSecretFactory">The factory that creates the HMAC symmetric key.</param>
        /// <param name="signingSettingsConfig">The action that configures the signing settings.</param>
        /// <returns>The <see cref="IServiceCollection" /> to which the registrations were added.</returns>
        [ExcludeFromCodeCoverage]
        [Obsolete("Please use the '" + nameof(AddHttpMessageSigning) + "' method without parameters, and use a " + nameof(IHttpMessageSigningBuilder) + " to continue configuration.")]
        public static IServiceCollection AddHMACHttpMessageSigning(
            this IServiceCollection services, 
            Func<IServiceProvider, KeyId> keyIdFactory,
            Func<IServiceProvider, string> hmacSecretFactory, 
            Action<SigningSettings> signingSettingsConfig) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (keyIdFactory == null) throw new ArgumentNullException(nameof(keyIdFactory));
            if (hmacSecretFactory == null) throw new ArgumentNullException(nameof(hmacSecretFactory));

            return services.AddHMACHttpMessageSigning(
                keyIdFactory,
                hmacSecretFactory,
                (prov, settings) => signingSettingsConfig(settings));
        }
        
        /// <summary>Adds http message signing registrations to the specified <see cref="IServiceCollection" />.</summary>
        /// <param name="keyIdFactory">The factory that creates the <see cref="T:Dalion.HttpMessageSigning.KeyId" /> that the server can use to identify the client application.</param>
        /// <param name="services">The <see cref="IServiceCollection" /> to add the registrations to.</param>
        /// <param name="hmacSecretFactory">The factory that creates the HMAC symmetric key.</param>
        /// <param name="signingSettingsConfig">The action that configures the signing settings.</param>
        /// <returns>The <see cref="IServiceCollection" /> to which the registrations were added.</returns>
        [ExcludeFromCodeCoverage]
        [Obsolete("Please use the '" + nameof(AddHttpMessageSigning) + "' method without parameters, and use a " + nameof(IHttpMessageSigningBuilder) + " to continue configuration.")]
        public static IServiceCollection AddHMACHttpMessageSigning(
            this IServiceCollection services, 
            Func<IServiceProvider, KeyId> keyIdFactory,
            Func<IServiceProvider, string> hmacSecretFactory, 
            Action<IServiceProvider, SigningSettings> signingSettingsConfig) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (keyIdFactory == null) throw new ArgumentNullException(nameof(keyIdFactory));
            if (hmacSecretFactory == null) throw new ArgumentNullException(nameof(hmacSecretFactory));

            return services.AddHttpMessageSigning(
                keyIdFactory,
                prov => {
                    var signingSettings = new SigningSettings {
                        SignatureAlgorithm = new HMACSignatureAlgorithm(hmacSecretFactory(prov), HashAlgorithmName.SHA512)
                    };
                    signingSettingsConfig?.Invoke(prov, signingSettings);
                    return signingSettings;
                });
        }
    }
}