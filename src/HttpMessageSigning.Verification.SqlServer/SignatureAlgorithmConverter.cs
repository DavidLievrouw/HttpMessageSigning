using System;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using Dalion.HttpMessageSigning.Utils;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    internal class SignatureAlgorithmConverter : ISignatureAlgorithmConverter {
        private readonly IStringProtectorFactory _stringProtectorFactory;
        
        public SignatureAlgorithmConverter(IStringProtectorFactory stringProtectorFactory) {
            _stringProtectorFactory = stringProtectorFactory ?? throw new ArgumentNullException(nameof(stringProtectorFactory));
        }
        
        public void SetSignatureAlgorithm(ClientDataRecord dataRecord, ISignatureAlgorithm signatureAlgorithm, SharedSecretEncryptionKey encryptionKey) {
            if (dataRecord == null) throw new ArgumentNullException(nameof(dataRecord));
            if (signatureAlgorithm == null) throw new ArgumentNullException(nameof(signatureAlgorithm));

            switch (signatureAlgorithm) {
                case RSASignatureAlgorithm rsa:
                    dataRecord.SigType = rsa.Name;
                    dataRecord.SigHashAlgorithm = rsa.HashAlgorithm.Name;
                    dataRecord.SigParameter = rsa.GetPublicKey().ToXml();
                    dataRecord.IsSigParameterEncrypted = false;
                    break;
                case ECDsaSignatureAlgorithm ecdsa:
                    dataRecord.SigType = ecdsa.Name;
                    dataRecord.SigHashAlgorithm = ecdsa.HashAlgorithm.Name;
                    dataRecord.SigParameter = ecdsa.GetPublicKey().ToXml();
                    dataRecord.IsSigParameterEncrypted = false;
                    break;
                case HMACSignatureAlgorithm hmac:
                    dataRecord.SigType = hmac.Name;
                    dataRecord.SigHashAlgorithm = hmac.HashAlgorithm.Name;
                    dataRecord.SigParameter = GetParameterWithEncryption(hmac, encryptionKey, out var isEncrypted);
                    dataRecord.IsSigParameterEncrypted = isEncrypted;
                    break;
                default:
                    throw new NotSupportedException($"The specified signature algorithm of type {signatureAlgorithm.GetType().Name} cannot be serialized.");
            }
        }
        
        public ISignatureAlgorithm ToSignatureAlgorithm(ClientDataRecord dataRecord, SharedSecretEncryptionKey encryptionKey) {
            if (dataRecord == null) throw new ArgumentNullException(nameof(dataRecord));
            
            switch (dataRecord.SigType) {
                case string str when str.Equals("rsa", StringComparison.OrdinalIgnoreCase):
                    using (var rsaForVerification = new RSACryptoServiceProvider()) {
                        rsaForVerification.FromXml(dataRecord.SigParameter);
                        var paramsForVerification = rsaForVerification.ExportParameters(false);
                        return SignatureAlgorithm.CreateForVerification(paramsForVerification, new HashAlgorithmName(dataRecord.SigHashAlgorithm));
                    }
                case string str when str.Equals("ecdsa", StringComparison.OrdinalIgnoreCase):
                    using (var ecdsaForVerification = ECDsa.Create()) {
                        ecdsaForVerification.FromXml(dataRecord.SigParameter);
                        var paramsForVerification = ecdsaForVerification.ExportParameters(false);
                        return SignatureAlgorithm.CreateForVerification(paramsForVerification, new HashAlgorithmName(dataRecord.SigHashAlgorithm));
                    }
                case string str when str.Equals("hmac", StringComparison.OrdinalIgnoreCase):
                    var unencryptedKey = GetUnencryptedParameter(dataRecord, encryptionKey);
                    return SignatureAlgorithm.CreateForVerification(unencryptedKey, new HashAlgorithmName(dataRecord.SigHashAlgorithm));
                default:
                    throw new NotSupportedException($"The specified signature algorithm type ({dataRecord.SigHashAlgorithm ?? "[null]"}) cannot be deserialized.");
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
        
        private string GetUnencryptedParameter(ClientDataRecord dataRecord, SharedSecretEncryptionKey encryptionKey) {
            if (encryptionKey == SharedSecretEncryptionKey.Empty) return dataRecord.SigParameter;
            
            if (!dataRecord.IsSigParameterEncrypted) return dataRecord.SigParameter; // The value in the data store is not encrypted

            var protector = _stringProtectorFactory.CreateSymmetric(encryptionKey);
            try {
                return protector.Unprotect(dataRecord.SigParameter);
            }
            catch (Exception ex) {
                throw new SecurityException("Something went wrong during acquisition of the unencrypted symmetric key. See inner exception for details.", ex);
            }
        }
    }
}