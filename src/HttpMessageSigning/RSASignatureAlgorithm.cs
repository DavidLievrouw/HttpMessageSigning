using System;
using System.Security.Cryptography;
using System.Text;

namespace Dalion.HttpMessageSigning {
    internal class RSASignatureAlgorithm : ISignatureAlgorithm {
        private readonly IHashAlgorithm _hashAlgorithm;
        private readonly RSACryptoServiceProvider _rsaForEncrypt;
        private readonly RSACryptoServiceProvider _rsaForSign;

        public RSASignatureAlgorithm(IHashAlgorithm hashAlgorithm, RSAParameters publicParameters) {
            _hashAlgorithm = hashAlgorithm ?? throw new ArgumentNullException(nameof(hashAlgorithm));
            _rsaForEncrypt = new RSACryptoServiceProvider();
            _rsaForEncrypt.ImportParameters(publicParameters);
        }
        
        public RSASignatureAlgorithm(IHashAlgorithm hashAlgorithm, RSAParameters publicParameters, RSAParameters privateParameters) {
            _hashAlgorithm = hashAlgorithm ?? throw new ArgumentNullException(nameof(hashAlgorithm));
            _rsaForEncrypt = new RSACryptoServiceProvider();
            _rsaForEncrypt.ImportParameters(publicParameters);
            _rsaForSign = new RSACryptoServiceProvider();
            _rsaForSign.ImportParameters(privateParameters);
        }

        public void Dispose() {
            _rsaForEncrypt?.Dispose();
            _rsaForSign?.Dispose();
        }

        public byte[] ComputeHash(string contentToSign) {
            var inputBytes = Encoding.UTF8.GetBytes(contentToSign);
            var encrypted = _rsaForEncrypt.Encrypt(inputBytes, false);
            var hashedData = _hashAlgorithm.ComputeHash(encrypted);
            return _rsaForSign.SignHash(hashedData, CryptoConfig.MapNameToOID(_hashAlgorithm.Id.ToString()));
        }

        public bool VerifySignature(string contentToSign, byte[] signature) {
            if (contentToSign == null) throw new ArgumentNullException(nameof(contentToSign));
            if (signature == null) throw new ArgumentNullException(nameof(signature));

            var signedBytes = Encoding.UTF8.GetBytes(contentToSign);
            var hashedData = _hashAlgorithm.ComputeHash(signedBytes);

            return _rsaForEncrypt.VerifyHash(hashedData, CryptoConfig.MapNameToOID(_hashAlgorithm.Id.ToString()), signature);
        }
    }
}