using System;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using Dalion.HttpMessageSigning.Utils;

namespace Dalion.HttpMessageSigning.Verification.FileSystem {
    internal class SignatureAlgorithmDataRecordConverter : ISignatureAlgorithmDataRecordConverter {
        private readonly IStringProtectorFactory _stringProtectorFactory;
        
        public SignatureAlgorithmDataRecordConverter(IStringProtectorFactory stringProtectorFactory) {
            _stringProtectorFactory = stringProtectorFactory ?? throw new ArgumentNullException(nameof(stringProtectorFactory));
        }
        
        public SignatureAlgorithmDataRecord FromSignatureAlgorithm(ISignatureAlgorithm signatureAlgorithm, SharedSecretEncryptionKey encryptionKey) {
            if (signatureAlgorithm == null) throw new ArgumentNullException(nameof(signatureAlgorithm));

            switch (signatureAlgorithm) {
                case RSASignatureAlgorithm rsa:
                    return new SignatureAlgorithmDataRecord {
                        Type = rsa.Name,
                        Hash = rsa.HashAlgorithm.Name,
                        Param = rsa.GetPublicKey().ToXml(),
                        Encrypted = false
                    };
                case ECDsaSignatureAlgorithm ecdsa:
                    return new SignatureAlgorithmDataRecord {
                        Type = ecdsa.Name,
                        Hash = ecdsa.HashAlgorithm.Name,
                        Param = ecdsa.GetPublicKey().ToXml(),
                        Encrypted = false
                    };
                case HMACSignatureAlgorithm hmac:
                    var paramValue = GetParameterWithEncryption(hmac, encryptionKey, out var isEncrypted);
                    var xmlString = new XElement("Secret", paramValue).ToString();
                    return new SignatureAlgorithmDataRecord {
                        Type = hmac.Name,
                        Hash = hmac.HashAlgorithm.Name,
                        Param = xmlString,
                        Encrypted = isEncrypted
                    };
                default:
                    throw new NotSupportedException($"The specified signature algorithm of type {signatureAlgorithm.GetType().Name} cannot be serialized.");
            }
        }
        
        public ISignatureAlgorithm ToSignatureAlgorithm(SignatureAlgorithmDataRecord dataRecord, SharedSecretEncryptionKey encryptionKey, int? recordVersion) {
            if (dataRecord == null) throw new ArgumentNullException(nameof(dataRecord));
            
            switch (dataRecord.Type) {
                case string str when str.Equals("rsa", StringComparison.OrdinalIgnoreCase):
                    using (var rsaForVerification = new RSACryptoServiceProvider()) {
                        rsaForVerification.FromXml(dataRecord.Param);
                        var paramsForVerification = rsaForVerification.ExportParameters(includePrivateParameters: false);
                        return SignatureAlgorithm.CreateForVerification(paramsForVerification, new HashAlgorithmName(dataRecord.Hash));
                    }
                case string str when str.Equals("ecdsa", StringComparison.OrdinalIgnoreCase):
                    using (var ecdsaForVerification = ECDsa.Create()) {
                        ecdsaForVerification.FromXml(dataRecord.Param);
                        var paramsForVerification = ecdsaForVerification.ExportParameters(includePrivateParameters: false);
                        return SignatureAlgorithm.CreateForVerification(paramsForVerification, new HashAlgorithmName(dataRecord.Hash));
                    }
                case string str when str.Equals("hmac", StringComparison.OrdinalIgnoreCase):
                    var unencryptedKey = GetUnencryptedParameter(dataRecord, encryptionKey);
                    return SignatureAlgorithm.CreateForVerification(unencryptedKey, new HashAlgorithmName(dataRecord.Hash));
                default:
                    throw new NotSupportedException($"The specified signature algorithm type ({dataRecord.Type ?? "[null]"}) cannot be deserialized.");
            }
        }
        
        private string GetParameterWithEncryption(HMACSignatureAlgorithm hmac, SharedSecretEncryptionKey encryptionKey, out bool isEncrypted) {
            var unencrypted = Encoding.UTF8.GetString(hmac.Key);

            if (encryptionKey == SharedSecretEncryptionKey.Empty) {
                isEncrypted = false;
                return unencrypted;
            }

            isEncrypted = true;
            
            var protector = _stringProtectorFactory.CreateSymmetric(encryptionKey);
            return protector.Protect(unencrypted);
        }
        
        private string GetUnencryptedParameter(SignatureAlgorithmDataRecord dataRecord, SharedSecretEncryptionKey encryptionKey) {
            var paramValue = XElement.Parse(dataRecord.Param).Value;
            
            if (encryptionKey == SharedSecretEncryptionKey.Empty) return paramValue;
            
            if (!dataRecord.Encrypted) return paramValue; // The value in the data store is not encrypted

            var protector = _stringProtectorFactory.CreateSymmetric(encryptionKey);
            try {
                return protector.Unprotect(paramValue);
            }
            catch (Exception ex) {
                throw new SecurityException("Something went wrong during acquisition of the unencrypted symmetric key. See inner exception for details.", ex);
            }
        }
    }
}