using System;
using System.Security.Cryptography;
using MongoDB.Bson.Serialization.Attributes;

namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    [BsonIgnoreExtraElements]
    internal class SignatureAlgorithmDataRecord {
        public string Type { get; set; }
        public string Parameter { get; set; }
        public string HashAlgorithm { get; set; }

        public static SignatureAlgorithmDataRecord FromSignatureAlgorithm(ISignatureAlgorithm signatureAlgorithm) {
            if (signatureAlgorithm == null) throw new ArgumentNullException(nameof(signatureAlgorithm));

            switch (signatureAlgorithm) {
                case RSASignatureAlgorithm rsa:
                    return new SignatureAlgorithmDataRecord {
                        Type = rsa.Name,
                        HashAlgorithm = rsa.HashAlgorithm.Name,
                        Parameter = rsa.GetPublicKey().ToXml()
                    };
                case HMACSignatureAlgorithm hmac:
                    return new SignatureAlgorithmDataRecord {
                        Type = hmac.Name,
                        HashAlgorithm = hmac.HashAlgorithm.Name,
                        Parameter = hmac.Secret
                    };
                default:
                    throw new NotSupportedException($"The specified signature algorithm of type {signatureAlgorithm.GetType().Name} cannot be serialized.");
            }
        }

        public ISignatureAlgorithm ToSignatureAlgorithm() {
            switch (Type) {
                case string str when str.Equals("rsa", StringComparison.OrdinalIgnoreCase):
                    using (var rsaForVerification = new RSACryptoServiceProvider()) {
                        rsaForVerification.FromXml(Parameter);
                        var paramsForVerification = rsaForVerification.ExportParameters(false);
                        return SignatureAlgorithm.CreateForVerification(paramsForVerification, new HashAlgorithmName(HashAlgorithm));
                    }
                case string str when str.Equals("hmac", StringComparison.OrdinalIgnoreCase):
                    return SignatureAlgorithm.CreateForVerification(Parameter, new HashAlgorithmName(HashAlgorithm));
                default:
                    throw new NotSupportedException($"The specified signature algorithm type ({Type ?? "[null]"}) cannot be deserialized.");
            }
        }
    }
}