﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Dalion.HttpMessageSigning.Verification.MongoDb.ClientStoreMigrations;
using Dalion.HttpMessageSigning.Verification.MongoDb.ClientStoreMigrations.V0002;
using Microsoft.Extensions.DependencyInjection;

namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    public static partial class Extensions {
        /// <summary>Adds http message signature verification registrations for MongoDb to the specified <see cref="IServiceCollection" />.</summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add the registrations to.</param>
        /// <param name="clientStoreSettings">The settings for the Mongo connection.</param>
        /// <returns>The <see cref="IServiceCollection" /> to which the registrations were added.</returns>
        [Obsolete("Please use the '" + nameof(UseMongoDbClientStore) + "' method of the '" + nameof(IHttpMessageSigningVerificationBuilder) + "' instead.")]
        [ExcludeFromCodeCoverage]
        public static IServiceCollection AddMongoDbClientStore(this IServiceCollection services, MongoDbSettings clientStoreSettings) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (clientStoreSettings == null) throw new ArgumentNullException(nameof(clientStoreSettings));

            return services.AddMongoDbClientStore(prov => (MongoDbClientStoreSettings) clientStoreSettings);
        }

        /// <summary>Adds http message signature verification registrations for MongoDb to the specified <see cref="IServiceCollection" />.</summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add the registrations to.</param>
        /// <param name="clientStoreSettingsFactory">The factory that creates the settings for the Mongo connection.</param>
        /// <returns>The <see cref="IServiceCollection" /> to which the registrations were added.</returns>
        [Obsolete("Please use the '" + nameof(UseMongoDbClientStore) + "' method of the '" + nameof(IHttpMessageSigningVerificationBuilder) + "' instead.")]
        [ExcludeFromCodeCoverage]
        public static IServiceCollection AddMongoDbClientStore(
            this IServiceCollection services,
            Func<IServiceProvider, MongoDbSettings> clientStoreSettingsFactory) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (clientStoreSettingsFactory == null) throw new ArgumentNullException(nameof(clientStoreSettingsFactory));

            return services.AddMongoDbClientStore(prov => (MongoDbClientStoreSettings) clientStoreSettingsFactory(prov));
        }

        /// <summary>Adds http message signature verification registrations for MongoDb to the specified <see cref="IServiceCollection" />.</summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add the registrations to.</param>
        /// <param name="clientStoreSettings">The settings for the Mongo connection.</param>
        /// <returns>The <see cref="IServiceCollection" /> to which the registrations were added.</returns>
        [Obsolete("Please use the '" + nameof(UseMongoDbClientStore) + "' method of the '" + nameof(IHttpMessageSigningVerificationBuilder) + "' instead.")]
        [ExcludeFromCodeCoverage]
        public static IServiceCollection AddMongoDbClientStore(this IServiceCollection services, MongoDbClientStoreSettings clientStoreSettings) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (clientStoreSettings == null) throw new ArgumentNullException(nameof(clientStoreSettings));

            return services.AddMongoDbClientStore(prov => clientStoreSettings);
        }

        /// <summary>Adds http message signature verification registrations for MongoDb to the specified <see cref="IServiceCollection" />.</summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add the registrations to.</param>
        /// <param name="clientStoreSettingsFactory">The factory that creates the settings for the Mongo connection.</param>
        /// <returns>The <see cref="IServiceCollection" /> to which the registrations were added.</returns>
        [Obsolete("Please use the '" + nameof(UseMongoDbClientStore) + "' method of the '" + nameof(IHttpMessageSigningVerificationBuilder) + "' instead.")]
        public static IServiceCollection AddMongoDbClientStore(
            this IServiceCollection services,
            Func<IServiceProvider, MongoDbClientStoreSettings> clientStoreSettingsFactory) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (clientStoreSettingsFactory == null) throw new ArgumentNullException(nameof(clientStoreSettingsFactory));

            return services
                // Services
                .AddMemoryCache()
                .AddSingleton<ISignatureAlgorithmDataRecordConverter, SignatureAlgorithmDataRecordConverter>()
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

                // ClientStore Migrations
                .AddSingleton<IClientStoreBaseliner, ClientStoreBaseliner>()
                .AddSingleton<ISemaphoreFactory, SemaphoreFactory>()
                .AddSingleton<IClientStoreMigrator>(prov =>
                    new OnlyOnceClientStoreMigrator(
                        new ClientStoreMigrator(
                            prov.GetRequiredService<IEnumerable<IClientStoreMigrationStep>>(),
                            prov.GetRequiredService<IClientStoreBaseliner>()),
                        prov.GetRequiredService<IClientStoreBaseliner>(),
                        prov.GetRequiredService<ISemaphoreFactory>()))
                .AddSingleton<IClientStoreMigrationStep, AddEncryptionSupportToClientsMigrationStep>()

                // The actual store
                .AddSingleton(prov => {
                    var mongoSettings = prov.GetRequiredService<MongoDbClientStoreSettings>();
                    var decorator = prov.GetRequiredService<ICachingClientStoreDecorator>();
                    var store = new MongoDbClientStore(
                        prov.GetRequiredService<IMongoDatabaseClientProvider>(),
                        mongoSettings.CollectionName,
                        mongoSettings.SharedSecretEncryptionKey,
                        prov.GetRequiredService<IClientStoreMigrator>(),
                        prov.GetRequiredService<ISignatureAlgorithmDataRecordConverter>());
                    return decorator.DecorateWithCaching(store, mongoSettings.ClientCacheEntryExpiration);
                });
        }
    }
}