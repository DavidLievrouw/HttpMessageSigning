using System;
using System.Diagnostics.CodeAnalysis;
using Dalion.HttpMessageSigning.Utils;
using Dalion.HttpMessageSigning.Verification.FileSystem.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Dalion.HttpMessageSigning.Verification.FileSystem {
    public static partial class Extensions {
        /// <summary>Configures HTTP message signature verification to use a file system-backed <see cref="INonceStore"/>.</summary>
        /// <param name="builder">The <see cref="IHttpMessageSigningVerificationBuilder" /> that is used to configure verification.</param>
        /// <param name="nonceStoreSettings">The settings for the file.</param>
        /// <returns>The <see cref="IHttpMessageSigningVerificationBuilder" /> that can be used to continue configuring the verification settings.</returns>
        [ExcludeFromCodeCoverage]
        public static IHttpMessageSigningVerificationBuilder UseFileSystemNonceStore(this IHttpMessageSigningVerificationBuilder builder, FileSystemNonceStoreSettings nonceStoreSettings) {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (nonceStoreSettings == null) throw new ArgumentNullException(nameof(nonceStoreSettings));

            return builder.UseFileSystemNonceStore(prov => nonceStoreSettings);
        }

        /// <summary>Configures HTTP message signature verification to use a file system-backed <see cref="INonceStore"/>.</summary>
        /// <param name="builder">The <see cref="IHttpMessageSigningVerificationBuilder" /> that is used to configure verification.</param>
        /// <param name="nonceStoreSettingsFactory">The factory that creates the settings for the file.</param>
        /// <returns>The <see cref="IHttpMessageSigningVerificationBuilder" /> that can be used to continue configuring the verification settings.</returns>
        public static IHttpMessageSigningVerificationBuilder UseFileSystemNonceStore(
            this IHttpMessageSigningVerificationBuilder builder,
            Func<IServiceProvider, FileSystemNonceStoreSettings> nonceStoreSettingsFactory) {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (nonceStoreSettingsFactory == null) throw new ArgumentNullException(nameof(nonceStoreSettingsFactory));

            builder.Services
                .AddMemoryCache()
                .AddSingleton<IFileReader, FileReader>()
                .AddSingleton<IFileWriter, FileWriter>()
                .AddSingleton<INonceDataRecordSerializer, NonceDataRecordSerializer>()
                .AddSingleton<ISemaphoreFactory, SemaphoreFactory>()
                .AddSingleton(prov => {
                    var settings = nonceStoreSettingsFactory(prov);
                    if (settings == null) throw new ValidationException($"Invalid {nameof(FileSystemNonceStoreSettings)} were specified.");
                    settings.Validate();
                    return settings;
                })
                .AddSingleton(prov => {
                    var settings = prov.GetRequiredService<FileSystemNonceStoreSettings>();
                    var decorator = prov.GetRequiredService<ICachingNonceStoreDecorator>();
                    var store = new LockingNonceStore(
                        new FileSystemNonceStore(
                            new LockingFileManager<NonceDataRecord>(
                                new NoncesFileManager(
                                    prov.GetRequiredService<IFileReader>(),
                                    prov.GetRequiredService<IFileWriter>(),
                                    settings.FilePath,
                                    prov.GetRequiredService<INonceDataRecordSerializer>()),
                                prov.GetRequiredService<ISemaphoreFactory>()),
                            prov.GetRequiredService<ISystemClock>()),
                        prov.GetRequiredService<ISemaphoreFactory>());
                    return decorator.DecorateWithCaching(store);
                });

            return builder;
        }
    }
}