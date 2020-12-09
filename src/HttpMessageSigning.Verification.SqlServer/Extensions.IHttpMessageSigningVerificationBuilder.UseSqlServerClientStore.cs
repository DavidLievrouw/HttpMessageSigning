using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    public static partial class Extensions {
        /// <summary>Configures HTTP message signature verification to use a SQL Server <see cref="IClientStore"/>.</summary>
        /// <param name="builder">The <see cref="IHttpMessageSigningVerificationBuilder" /> that is used to configure verification.</param>
        /// <param name="clientStoreSettings">The settings for the SQL Server connection.</param>
        /// <returns>The <see cref="IHttpMessageSigningVerificationBuilder" /> that can be used to continue configuring the verification settings.</returns>
        [ExcludeFromCodeCoverage]
        public static IHttpMessageSigningVerificationBuilder UseSqlServerClientStore(this IHttpMessageSigningVerificationBuilder builder, MongoDbClientStoreSettings clientStoreSettings) {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (clientStoreSettings == null) throw new ArgumentNullException(nameof(clientStoreSettings));

            return builder.UseSqlServerClientStore(prov => clientStoreSettings);
        }

        /// <summary>Configures HTTP message signature verification to use a SQL Server <see cref="IClientStore"/>.</summary>
        /// <param name="builder">The <see cref="IHttpMessageSigningVerificationBuilder" /> that is used to configure verification.</param>
        /// <param name="clientStoreSettingsFactory">The factory that creates the settings for the SQL Server connection.</param>
        /// <returns>The <see cref="IHttpMessageSigningVerificationBuilder" /> that can be used to continue configuring the verification settings.</returns>
        public static IHttpMessageSigningVerificationBuilder UseSqlServerClientStore(
            this IHttpMessageSigningVerificationBuilder builder,
            Func<IServiceProvider, MongoDbClientStoreSettings> clientStoreSettingsFactory) {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (clientStoreSettingsFactory == null) throw new ArgumentNullException(nameof(clientStoreSettingsFactory));

            builder.Services
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
                });

            return builder
                // The actual store
                .UseClientStore(prov => {
                    var mongoSettings = prov.GetRequiredService<MongoDbClientStoreSettings>();
                    return new CachingMongoDbClientStore(
                        new MongoDbClientStore(
                            prov.GetRequiredService<IMongoDatabaseClientProvider>(),
                            mongoSettings.CollectionName,
                            mongoSettings.SharedSecretEncryptionKey),
                        prov.GetRequiredService<IMemoryCache>(),
                        mongoSettings.ClientCacheEntryExpiration,
                        prov.GetRequiredService<IBackgroundTaskStarter>());
                });
        }
    }
}