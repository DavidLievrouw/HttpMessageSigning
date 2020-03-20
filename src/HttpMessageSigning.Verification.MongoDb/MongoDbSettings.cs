using System;

namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    [Obsolete("Please use the " + nameof(MongoDbClientStoreSettings) + " class instead.")]
    public class MongoDbSettings : MongoDbClientStoreSettings { }
}