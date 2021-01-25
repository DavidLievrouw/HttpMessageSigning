using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    public static partial class Extensions {
        /// <summary>Configures HTTP message signature verification to use a MongoDB <see cref="INonceStore"/>.</summary>
        /// <param name="builder">The <see cref="IHttpMessageSigningVerificationBuilder" /> that is used to configure verification.</param>
        /// <param name="nonceStoreSettings">The settings for the Mongo connection.</param>
        /// <returns>The <see cref="IHttpMessageSigningVerificationBuilder" /> that can be used to continue configuring the verification settings.</returns>
        [ExcludeFromCodeCoverage]
        public static IHttpMessageSigningVerificationBuilder UseMongoDbNonceStore(this IHttpMessageSigningVerificationBuilder builder, MongoDbNonceStoreSettings nonceStoreSettings) {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (nonceStoreSettings == null) throw new ArgumentNullException(nameof(nonceStoreSettings));

            return builder.UseMongoDbNonceStore(prov => nonceStoreSettings);
        }

        /// <summary>Configures HTTP message signature verification to use a MongoDB <see cref="INonceStore"/>.</summary>
        /// <param name="builder">The <see cref="IHttpMessageSigningVerificationBuilder" /> that is used to configure verification.</param>
        /// <param name="nonceStoreSettingsFactory">The factory that creates the settings for the Mongo connection.</param>
        /// <returns>The <see cref="IHttpMessageSigningVerificationBuilder" /> that can be used to continue configuring the verification settings.</returns>
        public static IHttpMessageSigningVerificationBuilder UseMongoDbNonceStore(
            this IHttpMessageSigningVerificationBuilder builder,
            Func<IServiceProvider, MongoDbNonceStoreSettings> nonceStoreSettingsFactory) {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (nonceStoreSettingsFactory == null) throw new ArgumentNullException(nameof(nonceStoreSettingsFactory));

            builder.Services
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

            return builder;
        }
    }
}