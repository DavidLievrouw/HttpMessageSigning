using System;
using Microsoft.Extensions.DependencyInjection;

namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    public static class Extensions {
        /// <summary>
        ///     Adds http message signature verification registrations to the specified
        ///     <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="services">
        ///     The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to add the
        ///     registrations to.
        /// </param>
        /// <param name="settings">The settings for the mongo connection.</param>
        /// <returns>
        ///     The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to which the registrations
        ///     were added.
        /// </returns>
        public static IServiceCollection AddHttpMessageSignatureVerificationWithMongoDbSupport(this IServiceCollection services, MongoDbSettings settings) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            return services.AddHttpMessageSignatureVerificationWithMongoDbSupport(prov => settings);
        }

        /// <summary>
        ///     Adds http message signature verification registrations to the specified
        ///     <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="services">
        ///     The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to add the
        ///     registrations to.
        /// </param>
        /// <param name="settingsFactory">The factory that creates the settings for the mongo connection.</param>
        /// <returns>
        ///     The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to which the registrations
        ///     were added.
        /// </returns>
        public static IServiceCollection AddHttpMessageSignatureVerificationWithMongoDbSupport(this IServiceCollection services,
            Func<IServiceProvider, MongoDbSettings> settingsFactory) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (settingsFactory == null) throw new ArgumentNullException(nameof(settingsFactory));

            return services
                .AddHttpMessageSignatureVerification(prov => {
                    var mongoSettings = settingsFactory(prov);
                    if (mongoSettings == null) throw new ValidationException($"Invalid {nameof(MongoDbSettings)} were specified.");
                    mongoSettings.Validate();
                    return new MongoDbClientStore(mongoSettings);
                });
        }
    }
}