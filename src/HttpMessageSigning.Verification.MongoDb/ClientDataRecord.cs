using System;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    [BsonIgnoreExtraElements]
    internal class ClientDataRecord {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public string Id { get; set; }

        public string Name { get; set; }
        public SignatureAlgorithmDataRecord SignatureAlgorithm { get; set; }
        public ClaimDataRecord[] Claims { get; set; }
        public double? ClockSkew { get; set; }
        public string RequestTargetEscaping { get; set; }
        public int? V { get; set; }
        public double? NonceLifetime { get; set; }

        [Obsolete("Please use the " + nameof(NonceLifetime) + " property instead.")]
        public double? NonceExpiration  { get; set; }
    }
}