using System;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;

namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    [BsonIgnoreExtraElements]
    internal class SignatureAlgorithmDataRecord {
        public string Type { get; set; }
        public string Parameter { get; set; }
        public string HashAlgorithm { get; set; }

        public static SignatureAlgorithmDataRecord FromSignatureAlgorithm(ISignatureAlgorithm signatureAlgorithm, string encryptionKey) {
            if (signatureAlgorithm == null) throw new ArgumentNullException(nameof(signatureAlgorithm));

            switch (signatureAlgorithm) {
                case RSASignatureAlgorithm rsa:
                    return new SignatureAlgorithmDataRecord {
                        Type = rsa.Name,
                        HashAlgorithm = rsa.HashAlgorithm.Name,
                        Parameter = rsa.GetPublicKey().ToXml()
                    };
                case ECDsaSignatureAlgorithm ecdsa:
                    return new SignatureAlgorithmDataRecord {
                        Type = ecdsa.Name,
                        HashAlgorithm = ecdsa.HashAlgorithm.Name,
                        Parameter = ecdsa.GetPublicKey().ToXml()
                    };
                case HMACSignatureAlgorithm hmac:
                    return new SignatureAlgorithmDataRecord {
                        Type = hmac.Name,
                        HashAlgorithm = hmac.HashAlgorithm.Name,
                        Parameter = GetKeyWithEncryption(hmac, encryptionKey)
                    };
                default:
                    throw new NotSupportedException($"The specified signature algorithm of type {signatureAlgorithm.GetType().Name} cannot be serialized.");
            }
        }

        public ISignatureAlgorithm ToSignatureAlgorithm(string encryptionKey, int? recordVersion) {
            switch (Type) {
                case string str when str.Equals("rsa", StringComparison.OrdinalIgnoreCase):
                    using (var rsaForVerification = new RSACryptoServiceProvider()) {
                        rsaForVerification.FromXml(Parameter);
                        var paramsForVerification = rsaForVerification.ExportParameters(false);
                        return SignatureAlgorithm.CreateForVerification(paramsForVerification, new HashAlgorithmName(HashAlgorithm));
                    }
                case string str when str.Equals("ecdsa", StringComparison.OrdinalIgnoreCase):
                    using (var ecdsaForVerification = ECDsa.Create()) {
                        ecdsaForVerification.FromXml(Parameter);
                        var paramsForVerification = ecdsaForVerification.ExportParameters(false);
                        return SignatureAlgorithm.CreateForVerification(paramsForVerification, new HashAlgorithmName(HashAlgorithm));
                    }
                case string str when str.Equals("hmac", StringComparison.OrdinalIgnoreCase):
                    var unencryptedKey = GetUnencryptedKey(Parameter, encryptionKey, recordVersion);
                    return SignatureAlgorithm.CreateForVerification(unencryptedKey, new HashAlgorithmName(HashAlgorithm));
                default:
                    throw new NotSupportedException($"The specified signature algorithm type ({Type ?? "[null]"}) cannot be deserialized.");
            }
        }

        private static string GetKeyWithEncryption(HMACSignatureAlgorithm hmac, string encryptionKey) {
            var unencrypted = Encoding.UTF8.GetString(hmac.Key);

            if (string.IsNullOrEmpty(encryptionKey)) return unencrypted;
            
            var protector = new SymmetricStringProtector(encryptionKey);
            return protector.Protect(unencrypted);
        }

        private static string GetUnencryptedKey(string parameter, string encryptionKey, int? recordVersion) {
            if (string.IsNullOrEmpty(encryptionKey)) return parameter;
            if (!recordVersion.HasValue || recordVersion.Value < 2) return parameter; // Encryption not yet supported

            var protector = new SymmetricStringProtector(encryptionKey);
            try {
                return protector.Unprotect(parameter);
            }
            catch (Exception ex) {
                throw new SecurityException("Something went wrong during acquisition of the unencrypted symmetric key. See inner exception for details.", ex);
            }
        }
    }
}