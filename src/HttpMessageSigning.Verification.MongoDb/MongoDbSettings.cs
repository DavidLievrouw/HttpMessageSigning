using System;

namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    public class MongoDbSettings {
        public string ConnectionString { get; set; }
        public string CollectionName { get; set; } = "clients";
        public TimeSpan ClientCacheEntryExpiration { get; set; } = TimeSpan.Zero;
        
        internal void Validate() {
            if (string.IsNullOrEmpty(ConnectionString)) throw new ValidationException($"The {nameof(MongoDbSettings)} do not specify a valid {nameof(ConnectionString)}.");
            if (string.IsNullOrEmpty(CollectionName)) throw new ValidationException($"The {nameof(MongoDbSettings)} do not specify a valid {nameof(CollectionName)}.");
        }
    }
}