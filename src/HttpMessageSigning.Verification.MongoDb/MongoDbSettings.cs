using System;

namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    /// <inheritdoc />
    [Obsolete("Please use the " + nameof(MongoDbClientStoreSettings) + " class instead.")]
    public class MongoDbSettings : MongoDbClientStoreSettings { }
}