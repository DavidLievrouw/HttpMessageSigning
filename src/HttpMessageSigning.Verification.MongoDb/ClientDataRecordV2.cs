using System;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    [BsonIgnoreExtraElements]
    internal class ClientDataRecordV2 {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public string Id { get; set; }

        public string Name { get; set; }
        public SignatureAlgorithmDataRecordV2 SignatureAlgorithm { get; set; }
        public ClaimDataRecordV2[] Claims { get; set; }
        public double? ClockSkew { get; set; }
        public string RequestTargetEscaping { get; set; }
        public int? V { get; set; }
        public double? NonceLifetime { get; set; }

        [Obsolete("Please use the " + nameof(NonceLifetime) + " property instead.")]
        public double? NonceExpiration  { get; set; }

        public static int GetV() {
            return 2;
        }
    }
}