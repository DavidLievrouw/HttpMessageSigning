using System;
using System.Diagnostics.CodeAnalysis;
using Dalion.HttpMessageSigning.Verification.FileSystem.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Dalion.HttpMessageSigning.Verification.FileSystem {
    /// <summary>
    ///     Extension methods for this library.
    /// </summary>
    public static partial class Extensions {
        /// <summary>Configures HTTP message signature verification to use a file system-backed <see cref="IClientStore"/>.</summary>
        /// <param name="builder">The <see cref="IHttpMessageSigningVerificationBuilder" /> that is used to configure verification.</param>
        /// <param name="clientStoreSettings">The settings for the file.</param>
        /// <returns>The <see cref="IHttpMessageSigningVerificationBuilder" /> that can be used to continue configuring the verification settings.</returns>
        [ExcludeFromCodeCoverage]
        public static IHttpMessageSigningVerificationBuilder UseFileSystemClientStore(
            this IHttpMessageSigningVerificationBuilder builder,
            FileSystemClientStoreSettings clientStoreSettings) {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (clientStoreSettings == null) throw new ArgumentNullException(nameof(clientStoreSettings));

            return builder.UseFileSystemClientStore(prov => clientStoreSettings);
        }

        /// <summary>Configures HTTP message signature verification to use a file system-backed <see cref="IClientStore"/>.</summary>
        /// <param name="builder">The <see cref="IHttpMessageSigningVerificationBuilder" /> that is used to configure verification.</param>
        /// <param name="clientStoreSettingsFactory">The factory that creates the settings for the file.</param>
        /// <returns>The <see cref="IHttpMessageSigningVerificationBuilder" /> that can be used to continue configuring the verification settings.</returns>
        public static IHttpMessageSigningVerificationBuilder UseFileSystemClientStore(
            this IHttpMessageSigningVerificationBuilder builder,
            Func<IServiceProvider, FileSystemClientStoreSettings> clientStoreSettingsFactory) {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (clientStoreSettingsFactory == null) throw new ArgumentNullException(nameof(clientStoreSettingsFactory));

            builder.Services
                .AddMemoryCache()
                .AddSingleton<IFileReader, FileReader>()
                .AddSingleton<IFileWriter, FileWriter>()
                .AddSingleton<IClientDataRecordSerializer, ClientDataRecordSerializer>()
                .AddSingleton<ILockFactory, LockFactory>()
                .AddSingleton<ISignatureAlgorithmDataRecordConverter, SignatureAlgorithmDataRecordConverter>()
                .AddSingleton(prov => {
                    var settings = clientStoreSettingsFactory(prov);
                    if (settings == null) throw new ValidationException($"Invalid {nameof(FileSystemClientStoreSettings)} were specified.");
                    settings.Validate();
                    return settings;
                });

            return builder
                // The actual store
                .UseClientStore(prov => {
                    var settings = prov.GetRequiredService<FileSystemClientStoreSettings>();
                    var decorator = prov.GetRequiredService<ICachingClientStoreDecorator>();
                    var store = new LockingClientStore(
                        new FileSystemClientStore(
                            new ClientsFileManager(
                                prov.GetRequiredService<IFileReader>(),
                                prov.GetRequiredService<IFileWriter>(),
                                settings.FilePath,
                                prov.GetRequiredService<IClientDataRecordSerializer>()),
                            prov.GetRequiredService<ISignatureAlgorithmDataRecordConverter>(),
                            settings.SharedSecretEncryptionKey),
                        prov.GetRequiredService<ILockFactory>());
                    return decorator.DecorateWithCaching(store, settings.ClientCacheEntryExpiration);
                });
        }
    }
}