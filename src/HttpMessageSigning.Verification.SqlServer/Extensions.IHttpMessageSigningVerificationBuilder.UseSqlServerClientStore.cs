﻿using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    /// <summary>
    ///     Extension methods for this library.
    /// </summary>
    public static partial class Extensions {
        /// <summary>Configures HTTP message signature verification to use a SQL Server <see cref="IClientStore"/>.</summary>
        /// <param name="builder">The <see cref="IHttpMessageSigningVerificationBuilder" /> that is used to configure verification.</param>
        /// <param name="clientStoreSettings">The settings for the SQL Server connection.</param>
        /// <returns>The <see cref="IHttpMessageSigningVerificationBuilder" /> that can be used to continue configuring the verification settings.</returns>
        [ExcludeFromCodeCoverage]
        public static IHttpMessageSigningVerificationBuilder UseSqlServerClientStore(this IHttpMessageSigningVerificationBuilder builder, SqlServerClientStoreSettings clientStoreSettings) {
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
            Func<IServiceProvider, SqlServerClientStoreSettings> clientStoreSettingsFactory) {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (clientStoreSettingsFactory == null) throw new ArgumentNullException(nameof(clientStoreSettingsFactory));

            builder.Services
                .AddMemoryCache()
                .AddSingleton<ISignatureAlgorithmConverter, SignatureAlgorithmConverter>()
                .AddSingleton(prov => {
                    var settings = clientStoreSettingsFactory(prov);
                    if (settings == null) throw new ValidationException($"Invalid {nameof(SqlServerClientStoreSettings)} were specified.");
                    settings.Validate();
                    return settings;
                });

            return builder
                // The actual store
                .UseClientStore(prov => {
                    var sqlSettings = prov.GetRequiredService<SqlServerClientStoreSettings>();
                    var decorator = prov.GetRequiredService<ICachingClientStoreDecorator>();
                    var store = new SqlServerClientStore(sqlSettings, prov.GetRequiredService<ISignatureAlgorithmConverter>());
                    return decorator.DecorateWithCaching(store, sqlSettings.ClientCacheEntryExpiration);
                });
        }
    }
}