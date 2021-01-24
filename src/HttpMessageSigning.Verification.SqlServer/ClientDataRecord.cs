using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace Dalion.HttpMessageSigning.Verification.SqlServer {
    internal class ClientDataRecord {
        public string Id { get; set; }
        public string Name { get; set; }
        public string SigType { get; set; }
        public string SigParameter { get; set; }
        public string SigHashAlgorithm { get; set; }
        public bool IsSigParameterEncrypted { get; set; }
        public double? ClockSkew { get; set; }
        public string RequestTargetEscaping { get; set; }
        public double? NonceLifetime { get; set; }
        public IList<ClaimDataRecord> Claims { get; set; }
        public int? V { get; set; }

        public int GetV() {
            return 1;
        }

        public void SetSignatureAlgorithm(ISignatureAlgorithm signatureAlgorithm, SharedSecretEncryptionKey encryptionKey) {
            if (signatureAlgorithm == null) throw new ArgumentNullException(nameof(signatureAlgorithm));

            switch (signatureAlgorithm) {
                case RSASignatureAlgorithm rsa:
                    SigType = rsa.Name;
                    SigHashAlgorithm = rsa.HashAlgorithm.Name;
                    SigParameter = rsa.GetPublicKey().ToXml();
                    IsSigParameterEncrypted = false;
                    break;
                case ECDsaSignatureAlgorithm ecdsa:
                    SigType = ecdsa.Name;
                    SigHashAlgorithm = ecdsa.HashAlgorithm.Name;
                    SigParameter = ecdsa.GetPublicKey().ToXml();
                    IsSigParameterEncrypted = false;
                    break;
                case HMACSignatureAlgorithm hmac:
                    SigType = hmac.Name;
                    SigHashAlgorithm = hmac.HashAlgorithm.Name;
                    SigParameter = GetParameterWithEncryption(hmac, encryptionKey, out var isEncrypted);
                    IsSigParameterEncrypted = isEncrypted;
                    break;
                default:
                    throw new NotSupportedException($"The specified signature algorithm of type {signatureAlgorithm.GetType().Name} cannot be serialized.");
            }
        }

        public ISignatureAlgorithm GetSignatureAlgorithm(SharedSecretEncryptionKey encryptionKey, int? recordVersion) {
            switch (SigType) {
                case string str when str.Equals("rsa", StringComparison.OrdinalIgnoreCase):
                    using (var rsaForVerification = new RSACryptoServiceProvider()) {
                        rsaForVerification.FromXml(SigParameter);
                        var paramsForVerification = rsaForVerification.ExportParameters(false);
                        return SignatureAlgorithm.CreateForVerification(paramsForVerification, new HashAlgorithmName(SigHashAlgorithm));
                    }
                case string str when str.Equals("ecdsa", StringComparison.OrdinalIgnoreCase):
                    using (var ecdsaForVerification = ECDsa.Create()) {
                        ecdsaForVerification.FromXml(SigParameter);
                        var paramsForVerification = ecdsaForVerification.ExportParameters(false);
                        return SignatureAlgorithm.CreateForVerification(paramsForVerification, new HashAlgorithmName(SigHashAlgorithm));
                    }
                case string str when str.Equals("hmac", StringComparison.OrdinalIgnoreCase):
                    var unencryptedKey = GetUnencryptedParameter(this, encryptionKey, recordVersion);
                    return SignatureAlgorithm.CreateForVerification(unencryptedKey, new HashAlgorithmName(SigHashAlgorithm));
                default:
                    throw new NotSupportedException($"The specified signature algorithm type ({SigHashAlgorithm ?? "[null]"}) cannot be deserialized.");
            }
        }

        private static string GetParameterWithEncryption(HMACSignatureAlgorithm hmac, SharedSecretEncryptionKey encryptionKey, out bool isEncrypted) {
            var unencrypted = Encoding.UTF8.GetString(hmac.Key);

            if (encryptionKey == SharedSecretEncryptionKey.Empty) {
                isEncrypted = false;
                return unencrypted;
            }

            isEncrypted = true;

            var protector = new SymmetricStringProtector(encryptionKey);
            return protector.Protect(unencrypted);
        }

        private static string GetUnencryptedParameter(ClientDataRecord dataRecord, SharedSecretEncryptionKey encryptionKey, int? recordVersion) {
            if (encryptionKey == SharedSecretEncryptionKey.Empty) return dataRecord.SigParameter;

            if (!dataRecord.IsSigParameterEncrypted) return dataRecord.SigParameter; // The value in the data store is not encrypted

            var protector = new SymmetricStringProtector(encryptionKey);
            try {
                return protector.Unprotect(dataRecord.SigParameter);
            }
            catch (Exception ex) {
                throw new SecurityException("Something went wrong during acquisition of the unencrypted symmetric key. See inner exception for details.", ex);
            }
        }
    }
}