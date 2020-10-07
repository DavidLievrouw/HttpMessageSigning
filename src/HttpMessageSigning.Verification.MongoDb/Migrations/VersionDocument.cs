using System;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace Dalion.HttpMessageSigning.Verification.MongoDb.Migrations {
    [BsonIgnoreExtraElements]
    internal class VersionDocument {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public string Id => "_version";
        public int Version { get; set; }
        public string StepName { get; set; }
        public DateTimeOffset Time { get; set; }
        public string PackageVersion { get; set; }
    }
}