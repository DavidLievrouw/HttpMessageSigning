using System;
using Microsoft.Extensions.DependencyInjection;

namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    public static partial class Extensions {
        /// <summary>
        ///     Adds http message signature verification registrations for MongoDb to the specified
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
        public static IServiceCollection AddMongoDbClientStore(this IServiceCollection services, MongoDbSettings settings) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            return services.AddMongoDbClientStore(prov => settings);
        }

        /// <summary>
        ///     Adds http message signature verification registrations for MongoDb to the specified
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
        public static IServiceCollection AddMongoDbClientStore(this IServiceCollection services,
            Func<IServiceProvider, MongoDbSettings> settingsFactory) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (settingsFactory == null) throw new ArgumentNullException(nameof(settingsFactory));

            return services
                .AddSingleton<IClientStore>(prov => {
                    var mongoSettings = settingsFactory(prov);
                    if (mongoSettings == null) throw new ValidationException($"Invalid {nameof(MongoDbSettings)} were specified.");
                    mongoSettings.Validate();
                    return new MongoDbClientStore(
                        new MongoDatabaseClientProvider(mongoSettings.ConnectionString),
                        mongoSettings.CollectionName);
                });
        }
    }
}