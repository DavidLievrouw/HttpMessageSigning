﻿using Xunit;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    [CollectionDefinition("MongoDbCollection")]
    public class MongoDbCollection : ICollectionFixture<MongoSetup> {
        // A class with no code, only used to define the collection
    }
}