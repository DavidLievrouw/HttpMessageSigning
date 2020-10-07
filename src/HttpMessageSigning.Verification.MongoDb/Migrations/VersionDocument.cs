using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace Dalion.HttpMessageSigning.Verification.MongoDb.Migrations {
    [BsonIgnoreExtraElements]
    internal class VersionDocument {
        internal const string VersionDocumentId = "_version";

        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public string Id => VersionDocumentId;
        public int Version { get; set; }
        public string StepName { get; set; }
        [BsonRepresentation(BsonType.String)]
        public DateTimeOffset Time { get; set; }
        public string PackageVersion { get; set; }
    }
}