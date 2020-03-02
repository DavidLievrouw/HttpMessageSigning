using System;
using System.Security.Cryptography;
using System.Text;

namespace Dalion.HttpMessageSigning {
    public class RSASignatureAlgorithm : ISignatureAlgorithm {
        private readonly RSA _rsa;
        private readonly HashAlgorithm _hasher;

        public RSASignatureAlgorithm(HashAlgorithmName hashAlgorithm, RSA rsa) {
            HashAlgorithm = hashAlgorithm;
            _hasher = System.Security.Cryptography.HashAlgorithm.Create(hashAlgorithm.Name);
            _rsa = rsa ?? throw new ArgumentNullException(nameof(rsa));
        }
        
        public RSASignatureAlgorithm(HashAlgorithmName hashAlgorithm, RSAParameters publicParameters) {
            HashAlgorithm = hashAlgorithm;
            _hasher = System.Security.Cryptography.HashAlgorithm.Create(hashAlgorithm.Name);
            _rsa = new RSACryptoServiceProvider();
            _rsa.ImportParameters(publicParameters);
        }

        public RSASignatureAlgorithm(HashAlgorithmName hashAlgorithm, RSAParameters publicParameters, RSAParameters privateParameters) {
            HashAlgorithm = hashAlgorithm;
            _hasher = System.Security.Cryptography.HashAlgorithm.Create(hashAlgorithm.Name);
            _rsa = new RSACryptoServiceProvider();
            _rsa.ImportParameters(publicParameters);
            _rsa.ImportParameters(privateParameters);
        }

        public HashAlgorithmName HashAlgorithm { get; }

        public RSAParameters GetPublicKey() {
            return _rsa.ExportParameters(false);
        }
        
        public void Dispose() {
            _rsa?.Dispose();
            _hasher?.Dispose();
        }

        public string Name => "RSA";

        public byte[] ComputeHash(string contentToSign) {
            var inputBytes = Encoding.UTF8.GetBytes(contentToSign);
            var hashedData = _hasher.ComputeHash(inputBytes);
            return _rsa.SignHash(hashedData, HashAlgorithm, RSASignaturePadding.Pkcs1);
        }

        public bool VerifySignature(string contentToSign, byte[] signature) {
            if (contentToSign == null) throw new ArgumentNullException(nameof(contentToSign));
            if (signature == null) throw new ArgumentNullException(nameof(signature));

            var signedBytes = Encoding.UTF8.GetBytes(contentToSign);
            var hashedData = _hasher.ComputeHash(signedBytes);

            return _rsa.VerifyHash(hashedData, signature, HashAlgorithm, RSASignaturePadding.Pkcs1);
        }
    }
}