using System;
using Dalion.HttpMessageSigning.Logging;
using Dalion.HttpMessageSigning.SigningString;
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
        /// <param name="signingSettings">The signing settings.</param>
        /// <returns>
        ///     The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to which the registrations
        ///     were added.
        /// </returns>
        public static IServiceCollection AddHttpMessageSigning(this IServiceCollection services, KeyId keyId, SigningSettings signingSettings) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (signingSettings == null) throw new ArgumentNullException(nameof(signingSettings));

            return services.AddHttpMessageSigning(keyId, prov => signingSettings);
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
        /// <param name="signingSettingsConfig">The action that configures the signing settings.</param>
        /// <returns>
        ///     The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to which the registrations
        ///     were added.
        /// </returns>
        public static IServiceCollection AddHttpMessageSigning(this IServiceCollection services, KeyId keyId, Action<SigningSettings> signingSettingsConfig) {
            if (services == null) throw new ArgumentNullException(nameof(services));

            return services.AddHttpMessageSigning(keyId, prov => {
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
        /// <param name="keyId">
        ///     The <see cref="T:Dalion.HttpMessageSigning.KeyId" /> that the server can use to identify the client
        ///     application.
        /// </param>
        /// <param name="signingSettingsFactory">The factory that creates the signing settings.</param>
        /// <returns>
        ///     The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to which the registrations
        ///     were added.
        /// </returns>
        public static IServiceCollection AddHttpMessageSigning(this IServiceCollection services, KeyId keyId, Func<IServiceProvider, SigningSettings> signingSettingsFactory) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (signingSettingsFactory == null) throw new ArgumentNullException(nameof(signingSettingsFactory));
            if (keyId == KeyId.Empty) throw new ArgumentException("The specified key id cannot be empty.", nameof(keyId));

            return services
                .AddSingleton(typeof(IHttpMessageSigningLogger<>), typeof(NetCoreHttpMessageSigningLogger<>))
                .AddSingleton<ISystemClock, RealSystemClock>()
                .AddSingleton<IBase64Converter, Base64Converter>()
                .AddSingleton<ISignatureCreator, SignatureCreator>()
                .AddSingleton<IAuthorizationHeaderParamCreator, AuthorizationHeaderParamCreator>()
                .AddSingleton<IHeaderAppenderFactory, HeaderAppenderFactory>()
                .AddSingleton<ISigningStringComposer, SigningStringComposer>()
                .AddSingleton<IRegisteredSignerSettingsStore, RegisteredSignerSettingsStore>()
                .AddTransient(prov => new RegisteredSigningSettings(keyId, signingSettingsFactory(prov)))
                .AddSingleton<IRequestSignerFactory>(prov => new RequestSignerFactory(
                    prov.GetRequiredService<ISignatureCreator>(),
                    prov.GetRequiredService<IAuthorizationHeaderParamCreator>(),
                    new DateSignatureHeaderEnsurer(),
                    new DigestSignatureHeaderEnsurer(
                        prov.GetRequiredService<IBase64Converter>()),
                    prov.GetRequiredService<ISystemClock>(),
                    prov.GetRequiredService<IHttpMessageSigningLogger<RequestSigner>>(),
                    prov.GetRequiredService<IRegisteredSignerSettingsStore>()));
        }
    }
}