﻿using System;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using Dalion.HttpMessageSigning.Utils;

namespace Dalion.HttpMessageSigning.Verification.MongoDb {
    internal class SignatureAlgorithmDataRecordConverter : ISignatureAlgorithmDataRecordConverter {
        private readonly IStringProtectorFactory _stringProtectorFactory;
        
        public SignatureAlgorithmDataRecordConverter(IStringProtectorFactory stringProtectorFactory) {
            _stringProtectorFactory = stringProtectorFactory ?? throw new ArgumentNullException(nameof(stringProtectorFactory));
        }
        
        public SignatureAlgorithmDataRecordV2 FromSignatureAlgorithm(ISignatureAlgorithm signatureAlgorithm, SharedSecretEncryptionKey encryptionKey) {
            if (signatureAlgorithm == null) throw new ArgumentNullException(nameof(signatureAlgorithm));

            switch (signatureAlgorithm) {
                case RSASignatureAlgorithm rsa:
                    return new SignatureAlgorithmDataRecordV2 {
                        Type = rsa.Name,
                        HashAlgorithm = rsa.HashAlgorithm.Name,
                        Parameter = rsa.GetPublicKey().ToXml(),
                        IsParameterEncrypted = false
                    };
                case ECDsaSignatureAlgorithm ecdsa:
                    return new SignatureAlgorithmDataRecordV2 {
                        Type = ecdsa.Name,
                        HashAlgorithm = ecdsa.HashAlgorithm.Name,
                        Parameter = ecdsa.GetPublicKey().ToXml(),
                        IsParameterEncrypted = false
                    };
                case HMACSignatureAlgorithm hmac:
                    return new SignatureAlgorithmDataRecordV2 {
                        Type = hmac.Name,
                        HashAlgorithm = hmac.HashAlgorithm.Name,
                        Parameter = GetParameterWithEncryption(hmac, encryptionKey, out var isEncrypted),
                        IsParameterEncrypted = isEncrypted
                    };
                default:
                    throw new NotSupportedException($"The specified signature algorithm of type {signatureAlgorithm.GetType().Name} cannot be serialized.");
            }
        }
        
        public ISignatureAlgorithm ToSignatureAlgorithm(SignatureAlgorithmDataRecordV2 dataRecord, SharedSecretEncryptionKey encryptionKey, int? recordVersion) {
            if (dataRecord == null) throw new ArgumentNullException(nameof(dataRecord));
            
            switch (dataRecord.Type) {
                case string str when str.Equals("rsa", StringComparison.OrdinalIgnoreCase):
                    using (var rsaForVerification = new RSACryptoServiceProvider()) {
                        rsaForVerification.FromXml(dataRecord.Parameter);
                        var paramsForVerification = rsaForVerification.ExportParameters(false);
                        return SignatureAlgorithm.CreateForVerification(paramsForVerification, new HashAlgorithmName(dataRecord.HashAlgorithm));
                    }
                case string str when str.Equals("ecdsa", StringComparison.OrdinalIgnoreCase):
                    using (var ecdsaForVerification = ECDsa.Create()) {
                        ecdsaForVerification.FromXml(dataRecord.Parameter);
                        var paramsForVerification = ecdsaForVerification.ExportParameters(false);
                        return SignatureAlgorithm.CreateForVerification(paramsForVerification, new HashAlgorithmName(dataRecord.HashAlgorithm));
                    }
                case string str when str.Equals("hmac", StringComparison.OrdinalIgnoreCase):
                    var unencryptedKey = GetUnencryptedParameter(dataRecord, encryptionKey, recordVersion);
                    return SignatureAlgorithm.CreateForVerification(unencryptedKey, new HashAlgorithmName(dataRecord.HashAlgorithm));
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
        
        private string GetUnencryptedParameter(SignatureAlgorithmDataRecordV2 dataRecord, SharedSecretEncryptionKey encryptionKey, int? recordVersion) {
            if (encryptionKey == SharedSecretEncryptionKey.Empty) return dataRecord.Parameter;
            if (!recordVersion.HasValue || recordVersion.Value < 2) return dataRecord.Parameter; // Encryption not yet supported
            
            if (!dataRecord.IsParameterEncrypted) return dataRecord.Parameter; // The value in the data store is not encrypted

            var protector = _stringProtectorFactory.CreateSymmetric(encryptionKey);
            try {
                return protector.Unprotect(dataRecord.Parameter);
            }
            catch (Exception ex) {
                throw new SecurityException("Something went wrong during acquisition of the unencrypted symmetric key. See inner exception for details.", ex);
            }
        }
    }
}