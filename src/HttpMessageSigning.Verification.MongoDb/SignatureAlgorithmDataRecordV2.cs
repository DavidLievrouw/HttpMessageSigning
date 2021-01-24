using MongoDB.Bson.Serialization.Attributes;

namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    [BsonIgnoreExtraElements]
    internal class SignatureAlgorithmDataRecordV2 {
        public string Type { get; set; }
        public string Parameter { get; set; }
        public string HashAlgorithm { get; set; }
        public bool IsParameterEncrypted { get; set; }
    }
}