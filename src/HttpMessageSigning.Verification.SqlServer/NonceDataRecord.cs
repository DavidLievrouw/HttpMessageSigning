using System;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    [BsonIgnoreExtraElements]
    internal class NonceDataRecord {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public string Id { get; set; }
        public string ClientId { get; set; }
        public string Value { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime Expiration { get; set; }
    }
}