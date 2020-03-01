namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    public class MongoDbSettings {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string CollectionName { get; set; } = "clients";
        
        internal void Validate() {
            if (string.IsNullOrEmpty(ConnectionString)) throw new ValidationException($"The {nameof(MongoDbSettings)} do not specify a valid {nameof(ConnectionString)}.");
            if (string.IsNullOrEmpty(DatabaseName)) throw new ValidationException($"The {nameof(MongoDbSettings)} do not specify a valid {nameof(DatabaseName)}.");
            if (string.IsNullOrEmpty(CollectionName)) throw new ValidationException($"The {nameof(MongoDbSettings)} do not specify a valid {nameof(CollectionName)}.");
        }
    }
}