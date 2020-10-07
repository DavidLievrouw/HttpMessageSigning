using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Dalion.HttpMessageSigning.Verification.MongoDb.Migrations;
using Microsoft.Extensions.Caching.Memory;
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
        /// <param name="clientStoreSettings">The settings for the Mongo connection.</param>
        /// <returns>
        ///     The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to which the registrations
        ///     were added.
        /// </returns>
        [Obsolete("Please use an overload that takes a " + nameof(MongoDbClientStoreSettings) + " parameter instead.")]
        [ExcludeFromCodeCoverage]
        public static IServiceCollection AddMongoDbClientStore(this IServiceCollection services, MongoDbSettings clientStoreSettings) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (clientStoreSettings == null) throw new ArgumentNullException(nameof(clientStoreSettings));

            return services.AddMongoDbClientStore(prov => (MongoDbClientStoreSettings) clientStoreSettings);
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
        [Obsolete("Please use an overload that takes a " + nameof(MongoDbClientStoreSettings) + " parameter instead.")]
        [ExcludeFromCodeCoverage]
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
        [ExcludeFromCodeCoverage]
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
                // Services
                .AddMemoryCache()
                .AddSingleton<ISystemClock, RealSystemClock>()
                .AddSingleton<IDelayer, Delayer>()
                .AddSingleton<IBackgroundTaskStarter, BackgroundTaskStarter>()
                .AddSingleton(prov => {
                    var settings = clientStoreSettingsFactory(prov);
                    if (settings == null) throw new ValidationException($"Invalid {nameof(MongoDbClientStoreSettings)} were specified.");
                    settings.Validate();
                    return settings;
                })
                .AddSingleton<IMongoDatabaseClientProvider>(prov => {
                    var mongoSettings = prov.GetRequiredService<MongoDbClientStoreSettings>();
                    return new MongoDatabaseClientProvider(mongoSettings.ConnectionString);
                })
                
                // Migrations
                .AddSingleton<IBaseliner, Baseliner>()
                .AddSingleton<ISemaphoreFactory, SemaphoreFactory>()
                .AddSingleton<IMigrator>(prov =>
                    new OnlyOnceMigrator(
                        new Migrator(
                            prov.GetRequiredService<IEnumerable<IMigrationStep>>(),
                            prov.GetRequiredService<IBaseliner>()),
                        prov.GetRequiredService<IBaseliner>(),
                        prov.GetRequiredService<ISemaphoreFactory>()))
                .AddSingleton<IMigrationStep, AddEncryptionSupportToClientsMigrationStep>()
                
                // The actual store
                .AddSingleton<IClientStore>(prov => {
                    var mongoSettings = prov.GetRequiredService<MongoDbClientStoreSettings>();
                    return new CachingMongoDbClientStore(
                        new MongoDbClientStore(
                            prov.GetRequiredService<IMongoDatabaseClientProvider>(),
                            mongoSettings.CollectionName,
                            mongoSettings.SharedSecretEncryptionKey,
                            prov.GetRequiredService<IMigrator>()),
                        prov.GetRequiredService<IMemoryCache>(),
                        mongoSettings.ClientCacheEntryExpiration,
                        prov.GetRequiredService<IBackgroundTaskStarter>());
                });
        }
    }
}