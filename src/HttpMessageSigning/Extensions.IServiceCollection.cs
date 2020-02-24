using System;
using Dalion.HttpMessageSigning.Logging;
using Dalion.HttpMessageSigning.Signing;
using Dalion.HttpMessageSigning.SigningString;
using Microsoft.Extensions.DependencyInjection;

namespace Dalion.HttpMessageSigning {
    public static class Extensions {
        /// <summary>
        ///     Adds http message signing registrations, using the specified configurator to the specified
        ///     <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="services">
        ///     The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to add the
        ///     registrations to.
        /// </param>
        /// <param name="config">The method that configures the signing algorithm.</param>
        /// <returns>
        ///     The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to which the registrations
        ///     were added.
        /// </returns>
        public static IServiceCollection AddHttpMessageSigning(this IServiceCollection services) {
            if (services == null) throw new ArgumentNullException(nameof(services));

            return services
                .AddSingleton(typeof(IHttpClientLogger<>), typeof(NetCoreHttpClientLogger<>))
                .AddSingleton<ISystemClock, RealSystemClock>()
                .AddSingleton<IBase64Converter, Base64Converter>()
                .AddSingleton<IKeyedHashAlgorithmFactory, KeyedHashAlgorithmFactory>()
                .AddSingleton<ISignatureCreator, SignatureCreator>()
                .AddSingleton<IAuthorizationHeaderParamCreator, AuthorizationHeaderParamCreator>()
                .AddSingleton<IHeaderAppenderFactory, HeaderAppenderFactory>()
                .AddSingleton<ISigningStringComposer, SigningStringComposer>()
                .AddSingleton<IRequestSigner, RequestSigner>();
        }
    }
}