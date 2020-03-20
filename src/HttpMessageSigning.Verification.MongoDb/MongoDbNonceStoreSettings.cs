namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    /// <summary>
    /// Represents settings for MongoDb storage of registered <see cref="Nonce"/> instances.
    /// </summary>
    public class MongoDbNonceStoreSettings {
        /// <summary>
        /// Gets or sets the connection string to the MongoDb database.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the name of the MongoDb collection that will contain nonces.
        /// </summary>
        public string CollectionName { get; set; } = "nonces";
        
        internal void Validate() {
            if (string.IsNullOrEmpty(ConnectionString)) throw new ValidationException($"The {nameof(MongoDbNonceStoreSettings)} do not specify a valid {nameof(ConnectionString)}.");
            if (string.IsNullOrEmpty(CollectionName)) throw new ValidationException($"The {nameof(MongoDbNonceStoreSettings)} do not specify a valid {nameof(CollectionName)}.");
        }
    }
}