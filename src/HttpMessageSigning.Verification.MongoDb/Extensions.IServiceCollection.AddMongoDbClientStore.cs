using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    public static partial class Extensions {
        [Obsolete("Please use an overload that takes a " + nameof(MongoDbClientStoreSettings) + " parameter instead.")]
        public static IServiceCollection AddMongoDbClientStore(this IServiceCollection services, MongoDbSettings clientStoreSettings) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (clientStoreSettings == null) throw new ArgumentNullException(nameof(clientStoreSettings));

            return services.AddMongoDbClientStore(prov => (MongoDbClientStoreSettings) clientStoreSettings);
        }
        
        [Obsolete("Please use an overload that takes a " + nameof(MongoDbClientStoreSettings) + " parameter instead.")]
        public static IServiceCollection AddMongoDbClientStore(this IServiceCollection services,
            Func<IServiceProvider, MongoDbSettings> clientStoreSettingsFactory) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (clientStoreSettingsFactory == null) throw new ArgumentNullException(nameof(clientStoreSettingsFactory));

            return services.AddMongoDbClientStore(prov => (MongoDbClientStoreSettings) clientStoreSettingsFactory(prov));
        }
    
        /// <summary>
        ///     Adds http message signature verification registrations for MongoDb to the specified
        ///     <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="services">
        ///     The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to add the
        ///     registrations to.
        /// </param>
        /// <param name="clientStoreSettings">The settings for the Mongo connection.</param>
        /// <returns>
        ///     The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to which the registrations
        ///     were added.
        /// </returns>
        public static IServiceCollection AddMongoDbClientStore(this IServiceCollection services, MongoDbClientStoreSettings clientStoreSettings) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (clientStoreSettings == null) throw new ArgumentNullException(nameof(clientStoreSettings));

            return services.AddMongoDbClientStore(prov => clientStoreSettings);
        }

        /// <summary>
        ///     Adds http message signature verification registrations for MongoDb to the specified
        ///     <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="services">
        ///     The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to add the
        ///     registrations to.
        /// </param>
        /// <param name="clientStoreSettingsFactory">The factory that creates the settings for the Mongo connection.</param>
        /// <returns>
        ///     The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to which the registrations
        ///     were added.
        /// </returns>
        public static IServiceCollection AddMongoDbClientStore(this IServiceCollection services,
            Func<IServiceProvider, MongoDbClientStoreSettings> clientStoreSettingsFactory) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (clientStoreSettingsFactory == null) throw new ArgumentNullException(nameof(clientStoreSettingsFactory));

            return services
                .AddMemoryCache()
                .AddSingleton<IClientStore>(prov => {
                    var mongoSettings = clientStoreSettingsFactory(prov);
                    if (mongoSettings == null) throw new ValidationException($"Invalid {nameof(MongoDbClientStoreSettings)} were specified.");
                    mongoSettings.Validate();
                    return new CachingMongoDbClientStore(
                        new MongoDbClientStore(
                            new MongoDatabaseClientProvider(mongoSettings.ConnectionString),
                            mongoSettings.CollectionName),
                        prov.GetRequiredService<IMemoryCache>(),
                        mongoSettings.ClientCacheEntryExpiration);
                });
        }
    }
}