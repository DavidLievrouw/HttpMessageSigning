using System;
using System.Security.Cryptography;
using System.Text;

namespace Dalion.HttpMessageSigning {
    public class RSASignatureAlgorithm : ISignatureAlgorithm {
        private readonly RSA _rsaForEncrypt;
        private readonly RSA _rsaForSign;
        private readonly HashAlgorithm _hasher;

        public RSASignatureAlgorithm(HashAlgorithmName hashAlgorithm, RSA rsa) {
            HashAlgorithm = hashAlgorithm;
            _hasher = System.Security.Cryptography.HashAlgorithm.Create(hashAlgorithm.Name);
            _rsaForEncrypt = rsa ?? throw new ArgumentNullException(nameof(rsa));
        }
        
        public RSASignatureAlgorithm(HashAlgorithmName hashAlgorithm, RSAParameters publicParameters) {
            HashAlgorithm = hashAlgorithm;
            _hasher = System.Security.Cryptography.HashAlgorithm.Create(hashAlgorithm.Name);
            _rsaForEncrypt = new RSACryptoServiceProvider();
            _rsaForEncrypt.ImportParameters(publicParameters);
        }

        public RSASignatureAlgorithm(HashAlgorithmName hashAlgorithm, RSAParameters publicParameters, RSAParameters privateParameters) : this(hashAlgorithm, publicParameters) {
            _rsaForSign = new RSACryptoServiceProvider();
            _rsaForSign.ImportParameters(privateParameters);
        }

        public HashAlgorithmName HashAlgorithm { get; }
        
        public void Dispose() {
            _rsaForEncrypt?.Dispose();
            _rsaForSign?.Dispose();
        }

        public string Name => "RSA";

        public byte[] ComputeHash(string contentToSign) {
            var inputBytes = Encoding.UTF8.GetBytes(contentToSign);
            var hashedData = _hasher.ComputeHash(inputBytes);
            return _rsaForSign.SignHash(hashedData, HashAlgorithm, RSASignaturePadding.Pkcs1);
        }

        public bool VerifySignature(string contentToSign, byte[] signature) {
            if (contentToSign == null) throw new ArgumentNullException(nameof(contentToSign));
            if (signature == null) throw new ArgumentNullException(nameof(signature));

            var signedBytes = Encoding.UTF8.GetBytes(contentToSign);
            var hashedData = _hasher.ComputeHash(signedBytes);

            return _rsaForEncrypt.VerifyHash(hashedData, signature, HashAlgorithm, RSASignaturePadding.Pkcs1);
        }
    }
}