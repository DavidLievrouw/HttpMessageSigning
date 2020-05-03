using System;
using System.Security.Cryptography;
using System.Text;

namespace Dalion.HttpMessageSigning {
    public class ECDsaSignatureAlgorithm : ISignatureAlgorithm {
        private readonly HashAlgorithm _hasher;
        private readonly ECDsa _ecdsa;

        public ECDsaSignatureAlgorithm(HashAlgorithmName hashAlgorithm, ECDsa ecdsa) {
            HashAlgorithm = hashAlgorithm;
            _hasher = System.Security.Cryptography.HashAlgorithm.Create(hashAlgorithm.Name);
            _ecdsa = ecdsa ?? throw new ArgumentNullException(nameof(ecdsa));
        }

        public HashAlgorithmName HashAlgorithm { get; }

        public void Dispose() {
            _ecdsa?.Dispose();
            _hasher?.Dispose();
        }

        public string Name => "ECDsa";

        public byte[] ComputeHash(string contentToSign) {
            var inputBytes = Encoding.UTF8.GetBytes(contentToSign);
            var hashedData = _hasher.ComputeHash(inputBytes);
            return _ecdsa.SignHash(hashedData);
        }

        public bool VerifySignature(string contentToSign, byte[] signature) {
            if (contentToSign == null) throw new ArgumentNullException(nameof(contentToSign));
            if (signature == null) throw new ArgumentNullException(nameof(signature));

            var signedBytes = Encoding.UTF8.GetBytes(contentToSign);
            var hashedData = _hasher.ComputeHash(signedBytes);

            return _ecdsa.VerifyHash(hashedData, signature);
        }
        
        public static ECDsaSignatureAlgorithm CreateForSigning(HashAlgorithmName hashAlgorithmName, ECParameters privateParameters) {
            var ecdsa = ECDsa.Create();
            ecdsa.ImportParameters(privateParameters);
            return new ECDsaSignatureAlgorithm(hashAlgorithmName, ecdsa);
        }

        public static ECDsaSignatureAlgorithm CreateForVerification(HashAlgorithmName hashAlgorithmName, ECParameters publicParameters) {
            var ecdsa = ECDsa.Create();
            ecdsa.ImportParameters(publicParameters);
            return new ECDsaSignatureAlgorithm(hashAlgorithmName, ecdsa);
        }
        
        public ECParameters GetPublicKey() {
            return _ecdsa.ExportParameters(false);
        }
    }
}