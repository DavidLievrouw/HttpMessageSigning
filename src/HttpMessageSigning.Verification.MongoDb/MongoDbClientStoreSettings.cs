using System;

namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    /// <summary>
    /// Represents settings for MongoDb storage of registered <see cref="Client"/> instances.
    /// </summary>
    public class MongoDbClientStoreSettings {
        /// <summary>
        /// Gets or sets the connection string to the MongoDb database.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the name of the MongoDb collection that will contain registered clients.
        /// </summary>
        public string CollectionName { get; set; } = "clients";

        /// <summary>
        /// Gets or sets the encryption key for the shared secrets.
        /// </summary>
        /// <remarks>This only applies to signature algorithms that use symmetric keys, e.g. HMAC. Set this value to <see langword="null" /> to disable encryption.</remarks>
        public string SharedSecretEncryptionKey { get; set; }
        
        /// <summary>
        /// Gets or sets the time that client queries are cached in memory.
        /// </summary>
        /// <remarks>Set to <see cref="TimeSpan.Zero"/> to disable caching.</remarks>
        public TimeSpan ClientCacheEntryExpiration { get; set; } = TimeSpan.Zero;
        
        internal void Validate() {
            if (string.IsNullOrEmpty(ConnectionString)) throw new ValidationException($"The {nameof(MongoDbClientStoreSettings)} do not specify a valid {nameof(ConnectionString)}.");
            if (string.IsNullOrEmpty(CollectionName)) throw new ValidationException($"The {nameof(MongoDbClientStoreSettings)} do not specify a valid {nameof(CollectionName)}.");
        }
    }
}