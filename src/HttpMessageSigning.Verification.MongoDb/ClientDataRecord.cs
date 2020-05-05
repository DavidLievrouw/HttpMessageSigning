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
        public double? NonceExpiration { get; set; }
        public double? ClockSkew { get; set; }
    }
}