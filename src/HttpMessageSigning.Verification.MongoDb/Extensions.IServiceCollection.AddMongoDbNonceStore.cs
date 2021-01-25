﻿using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    public static partial class Extensions {
        /// <summary>Adds http message signature verification registrations for MongoDb to the specified <see cref="IServiceCollection" />.</summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add the registrations to.</param>
        /// <param name="nonceStoreSettings">The settings for the Mongo connection.</param>
        /// <returns>The <see cref="IServiceCollection" /> to which the registrations were added.</returns>
        [ExcludeFromCodeCoverage]
        [Obsolete("Please use the '" + nameof(UseMongoDbNonceStore) + "' method of the '" + nameof(IHttpMessageSigningVerificationBuilder) + "' instead.")]
        public static IServiceCollection AddMongoDbNonceStore(this IServiceCollection services, MongoDbNonceStoreSettings nonceStoreSettings) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (nonceStoreSettings == null) throw new ArgumentNullException(nameof(nonceStoreSettings));

            return services.AddMongoDbNonceStore(prov => nonceStoreSettings);
        }

        /// <summary>Adds http message signature verification registrations for MongoDb to the specified <see cref="IServiceCollection" />.</summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add the registrations to.</param>
        /// <param name="nonceStoreSettingsFactory">The factory that creates the settings for the Mongo connection.</param>
        /// <returns>The <see cref="IServiceCollection" /> to which the registrations were added.</returns>
        [Obsolete("Please use the '" + nameof(UseMongoDbNonceStore) + "' method of the '" + nameof(IHttpMessageSigningVerificationBuilder) + "' instead.")]
        public static IServiceCollection AddMongoDbNonceStore(
            this IServiceCollection services,
            Func<IServiceProvider, MongoDbNonceStoreSettings> nonceStoreSettingsFactory) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (nonceStoreSettingsFactory == null) throw new ArgumentNullException(nameof(nonceStoreSettingsFactory));

            return services
                .AddMemoryCache()
                .AddSingleton(prov => {
                    var mongoSettings = nonceStoreSettingsFactory(prov);
                    if (mongoSettings == null) throw new ValidationException($"Invalid {nameof(MongoDbNonceStoreSettings)} were specified.");
                    mongoSettings.Validate();
                    var decorator = prov.GetRequiredService<ICachingNonceStoreDecorator>();
                    var store = new MongoDbNonceStore(
                        new MongoDatabaseClientProvider(mongoSettings.ConnectionString),
                        mongoSettings.CollectionName);
                    return decorator.DecorateWithCaching(store);
                });
        }
    }
}