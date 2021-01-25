using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Dalion.HttpMessageSigning.Verification.MongoDb.ClientStoreMigrations;
using Dalion.HttpMessageSigning.Verification.MongoDb.ClientStoreMigrations.V0002;
using Microsoft.Extensions.DependencyInjection;

namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    /// <summary>
    ///     Extension methods for this library.
    /// </summary>
    public static partial class Extensions {
        /// <summary>Configures HTTP message signature verification to use a MongoDB <see cref="IClientStore"/>.</summary>
        /// <param name="builder">The <see cref="IHttpMessageSigningVerificationBuilder" /> that is used to configure verification.</param>
        /// <param name="clientStoreSettings">The settings for the Mongo connection.</param>
        /// <returns>The <see cref="IHttpMessageSigningVerificationBuilder" /> that can be used to continue configuring the verification settings.</returns>
        [ExcludeFromCodeCoverage]
        public static IHttpMessageSigningVerificationBuilder UseMongoDbClientStore(this IHttpMessageSigningVerificationBuilder builder, MongoDbClientStoreSettings clientStoreSettings) {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (clientStoreSettings == null) throw new ArgumentNullException(nameof(clientStoreSettings));

            return builder.UseMongoDbClientStore(prov => clientStoreSettings);
        }

        /// <summary>Configures HTTP message signature verification to use a MongoDB <see cref="IClientStore"/>.</summary>
        /// <param name="builder">The <see cref="IHttpMessageSigningVerificationBuilder" /> that is used to configure verification.</param>
        /// <param name="clientStoreSettingsFactory">The factory that creates the settings for the Mongo connection.</param>
        /// <returns>The <see cref="IHttpMessageSigningVerificationBuilder" /> that can be used to continue configuring the verification settings.</returns>
        public static IHttpMessageSigningVerificationBuilder UseMongoDbClientStore(
            this IHttpMessageSigningVerificationBuilder builder,
            Func<IServiceProvider, MongoDbClientStoreSettings> clientStoreSettingsFactory) {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (clientStoreSettingsFactory == null) throw new ArgumentNullException(nameof(clientStoreSettingsFactory));

            builder.Services
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
                .AddSingleton<IClientStoreMigrationStep, AddEncryptionSupportToClientsMigrationStep>();

            return builder
                // The actual store
                .UseClientStore(prov => {
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