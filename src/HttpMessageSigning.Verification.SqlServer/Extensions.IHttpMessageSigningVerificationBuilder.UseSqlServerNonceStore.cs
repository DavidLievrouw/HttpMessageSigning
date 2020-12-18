﻿using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    public static partial class Extensions {
        /// <summary>Configures HTTP message signature verification to use a SQL Server <see cref="INonceStore"/>.</summary>
        /// <param name="builder">The <see cref="IHttpMessageSigningVerificationBuilder" /> that is used to configure verification.</param>
        /// <param name="nonceStoreSettings">The settings for the SQL Server connection.</param>
        /// <returns>The <see cref="IHttpMessageSigningVerificationBuilder" /> that can be used to continue configuring the verification settings.</returns>
        [ExcludeFromCodeCoverage]
        public static IHttpMessageSigningVerificationBuilder UseSqlServerNonceStore(this IHttpMessageSigningVerificationBuilder builder, SqlServerNonceStoreSettings nonceStoreSettings) {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (nonceStoreSettings == null) throw new ArgumentNullException(nameof(nonceStoreSettings));

            return builder.UseSqlServerNonceStore(prov => nonceStoreSettings);
        }

        /// <summary>Configures HTTP message signature verification to use a SQL Server <see cref="INonceStore"/>.</summary>
        /// <param name="builder">The <see cref="IHttpMessageSigningVerificationBuilder" /> that is used to configure verification.</param>
        /// <param name="nonceStoreSettingsFactory">The factory that creates the settings for the SQL Server connection.</param>
        /// <returns>The <see cref="IHttpMessageSigningVerificationBuilder" /> that can be used to continue configuring the verification settings.</returns>
        public static IHttpMessageSigningVerificationBuilder UseSqlServerNonceStore(
            this IHttpMessageSigningVerificationBuilder builder,
            Func<IServiceProvider, SqlServerNonceStoreSettings> nonceStoreSettingsFactory) {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (nonceStoreSettingsFactory == null) throw new ArgumentNullException(nameof(nonceStoreSettingsFactory));

            builder.Services
                .AddMemoryCache()
                .AddSingleton(prov => {
                    var sqlSettings = nonceStoreSettingsFactory(prov);
                    if (sqlSettings == null) throw new ValidationException($"Invalid {nameof(SqlServerNonceStoreSettings)} were specified.");
                    sqlSettings.Validate();
                    return sqlSettings;
                })
                .AddSingleton<IExpiredNoncesCleaner>(prov => {
                    var sqlSettings = prov.GetRequiredService<SqlServerNonceStoreSettings>();
                    return new ExpiredNoncesCleaner(
                        sqlSettings.ConnectionString,
                        sqlSettings.TableName,
                        prov.GetRequiredService<IBackgroundTaskStarter>(),
                        prov.GetRequiredService<ISystemClock>(),
                        sqlSettings.ExpiredNoncesCleanUpInterval);
                })
                .AddSingleton<INonceStore>(prov => {
                    var sqlSettings = prov.GetRequiredService<SqlServerNonceStoreSettings>();
                    return new CachingSqlServerNonceStore(
                        new SqlServerNonceStore(
                            sqlSettings.ConnectionString,
                            sqlSettings.TableName,
                            prov.GetRequiredService<IExpiredNoncesCleaner>()),
                        prov.GetRequiredService<IMemoryCache>());
                });

            return builder;
        }
    }
}