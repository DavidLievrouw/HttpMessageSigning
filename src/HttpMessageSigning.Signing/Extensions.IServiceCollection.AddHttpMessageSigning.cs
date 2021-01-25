﻿using System;
using System.Diagnostics.CodeAnalysis;
using Dalion.HttpMessageSigning.SigningString;
using Dalion.HttpMessageSigning.SigningString.RequestTarget;
using Dalion.HttpMessageSigning.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace Dalion.HttpMessageSigning.Signing {
    public static partial class Extensions {
        /// <summary>Adds http message signing registrations to the specified <see cref="IServiceCollection" />.</summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add the registrations to.</param>
        /// <returns>The <see cref="IHttpMessageSigningBuilder" /> that can be used to configure the signing settings.</returns>
        [ExcludeFromCodeCoverage]
        public static IHttpMessageSigningBuilder AddHttpMessageSigning(this IServiceCollection services) {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services = services.AddHttpMessageSigningPlumbing();
            
            return new HttpMessageSigningBuilder(services);
        }
        
        /// <summary>Adds http message signing registrations to the specified <see cref="IServiceCollection" />.</summary>
        /// <param name="keyId">The <see cref="KeyId" /> that the server can use to identify the client application.</param>
        /// <param name="services">The <see cref="IServiceCollection" /> to add the registrations to.</param>
        /// <param name="signingSettings">The signing settings.</param>
        /// <returns>The <see cref="IServiceCollection" /> to which the registrations were added.</returns>
        [ExcludeFromCodeCoverage]
        public static IServiceCollection AddHttpMessageSigning(this IServiceCollection services, KeyId keyId, SigningSettings signingSettings) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (signingSettings == null) throw new ArgumentNullException(nameof(signingSettings));

            return services.AddHttpMessageSigning(keyId, prov => signingSettings);
        }

        /// <summary>Adds http message signing registrations to the specified <see cref="IServiceCollection" />.</summary>
        /// <param name="keyId">The <see cref="T:Dalion.HttpMessageSigning.KeyId" /> that the server can use to identify the client application.</param>
        /// <param name="services">The <see cref="IServiceCollection" /> to add the registrations to.</param>
        /// <param name="signingSettingsConfig">The action that configures the signing settings.</param>
        /// <returns>The <see cref="IServiceCollection" /> to which the registrations were added.</returns>
        [ExcludeFromCodeCoverage]
        public static IServiceCollection AddHttpMessageSigning(this IServiceCollection services, KeyId keyId, Action<SigningSettings> signingSettingsConfig) {
            if (services == null) throw new ArgumentNullException(nameof(services));

            return services.AddHttpMessageSigning(keyId, prov => {
                var newSettings = new SigningSettings();
                signingSettingsConfig?.Invoke(newSettings);
                return newSettings;
            });
        }

        /// <summary>Adds http message signing registrations to the specified <see cref="IServiceCollection" />.</summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add the registrations to.</param>
        /// <param name="keyId">The <see cref="T:Dalion.HttpMessageSigning.KeyId" /> that the server can use to identify the client application.</param>
        /// <param name="signingSettingsFactory">The factory that creates the signing settings.</param>
        /// <returns>The <see cref="IServiceCollection" /> to which the registrations were added.</returns>
        [ExcludeFromCodeCoverage]
        public static IServiceCollection AddHttpMessageSigning(this IServiceCollection services, KeyId keyId, Func<IServiceProvider, SigningSettings> signingSettingsFactory) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (signingSettingsFactory == null) throw new ArgumentNullException(nameof(signingSettingsFactory));
            if (keyId == KeyId.Empty) throw new ArgumentException("The specified key id cannot be empty.", nameof(keyId));

            return services.AddHttpMessageSigning(prov => keyId, signingSettingsFactory);
        }
        
        /// <summary>Adds http message signing registrations to the specified <see cref="IServiceCollection" />.</summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add the registrations to.</param>
        /// <param name="keyIdFactory">The factory that creates the <see cref="T:Dalion.HttpMessageSigning.KeyId" /> that the server can use to identify the client application.</param>
        /// <param name="signingSettingsConfig">The action that configures the signing settings.</param>
        /// <returns>The <see cref="IServiceCollection" /> to which the registrations were added.</returns>
        [ExcludeFromCodeCoverage]
        public static IServiceCollection AddHttpMessageSigning(
            this IServiceCollection services, 
            Func<IServiceProvider, KeyId> keyIdFactory, 
            Action<SigningSettings> signingSettingsConfig) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (keyIdFactory == null) throw new ArgumentNullException(nameof(keyIdFactory));

            return services.AddHttpMessageSigning(keyIdFactory, prov => {
                var newSettings = new SigningSettings();
                signingSettingsConfig?.Invoke(newSettings);
                return newSettings;
            });
        }
        
        /// <summary>Adds http message signing registrations to the specified <see cref="IServiceCollection" />.</summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add the registrations to.</param>
        /// <param name="keyIdFactory">The factory that creates the <see cref="T:Dalion.HttpMessageSigning.KeyId" /> that the server can use to identify the client application.</param>
        /// <param name="signingSettingsFactory">The factory that creates the signing settings.</param>
        /// <returns>The <see cref="IServiceCollection" /> to which the registrations were added.</returns>
        public static IServiceCollection AddHttpMessageSigning(this IServiceCollection services, Func<IServiceProvider, KeyId> keyIdFactory, Func<IServiceProvider, SigningSettings> signingSettingsFactory) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (keyIdFactory == null) throw new ArgumentNullException(nameof(keyIdFactory));
            if (signingSettingsFactory == null) throw new ArgumentNullException(nameof(signingSettingsFactory));
            
            return services
                .AddHttpMessageSigningPlumbing()
                .AddTransient(prov => new RegisteredSigningSettings(keyIdFactory(prov), signingSettingsFactory(prov)));
        }
        
        private static IServiceCollection AddHttpMessageSigningPlumbing(this IServiceCollection services) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            
            return services
                .AddSingleton<ISystemClock, RealSystemClock>()
                .AddSingleton<INonceAppender, NonceAppender>()
                .AddSingleton<INonceGenerator, NonceGenerator>()
                .AddSingleton<IBase64Converter, Base64Converter>()
                .AddSingleton<ISignatureCreator, SignatureCreator>()
                .AddSingleton<ISigningSettingsSanitizer, SigningSettingsSanitizer>()
                .AddSingleton<IAuthorizationHeaderParamCreator, AuthorizationHeaderParamCreator>()
                .AddSingleton<IHeaderAppenderFactory, HeaderAppenderFactory>()
                .AddSingleton<IRequestTargetEscaper>(provider => new CompositeRequestTargetEscaper(
                    new RFC3986RequestTargetEscaper(), 
                    new RFC2396RequestTargetEscaper(), 
                    new UnescapedRequestTargetEscaper(), 
                    new OriginalStringRequestTargetEscaper()))
                .AddSingleton<ISigningStringComposer, SigningStringComposer>()
                .AddSingleton<IRegisteredSignerSettingsStore, RegisteredSignerSettingsStore>()
                .AddSingleton<ISignatureHeaderEnsurer>(provider => new CompositeSignatureHeaderEnsurer(
                    new DateSignatureHeaderEnsurer(),
                    new DigestSignatureHeaderEnsurer(provider.GetRequiredService<IBase64Converter>())))
                .AddSingleton<ISigningStringCompositionRequestFactory, SigningStringCompositionRequestFactory>()
                .AddSingleton<IRequestSignerFactory, RequestSignerFactory>();
        }
    }
}