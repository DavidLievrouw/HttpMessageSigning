using System;

namespace Dalion.HttpMessageSigning.Verification.FileSystem {
    /// <summary>
    ///     Represents settings for FileSystem storage of registered <see cref="Client" /> instances.
    /// </summary>
    public class FileSystemClientStoreSettings {
        /// <summary>
        ///     Gets or sets the path to the data file. This path should contain the file name with extension.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        ///     Gets or sets the encryption key for the shared secrets.
        /// </summary>
        /// <remarks>This only applies to signature algorithms that use symmetric keys, e.g. HMAC. Set this value to <see langword="null" /> to disable encryption.</remarks>
        public SharedSecretEncryptionKey SharedSecretEncryptionKey { get; set; } = SharedSecretEncryptionKey.Empty;

        /// <summary>
        ///     Gets or sets the time that client queries are cached in memory.
        /// </summary>
        /// <remarks>Set to <see cref="TimeSpan.Zero" /> to disable caching.</remarks>
        public TimeSpan ClientCacheEntryExpiration { get; set; } = TimeSpan.Zero;

        internal void Validate() {
            if (string.IsNullOrEmpty(FilePath)) {
                throw new ValidationException($"The {nameof(FileSystemClientStoreSettings)} do not specify a valid {nameof(FilePath)}.");
            }
        }
    }
}