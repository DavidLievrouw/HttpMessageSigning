using System;
using System.Security.Cryptography;
using System.Text;

namespace Dalion.HttpMessageSigning {
    public class RSASignatureAlgorithm : ISignatureAlgorithm {
        private readonly RSACryptoServiceProvider _rsaForEncrypt;
        private readonly RSACryptoServiceProvider _rsaForSign;
        private readonly IHashAlgorithm _hashAlgorithm;

        public RSASignatureAlgorithm(HashAlgorithm hashAlgorithm, RSAParameters publicParameters) {
            if (hashAlgorithm == HashAlgorithm.None) throw new ArgumentException("A hash algorithm must be specified.", nameof(hashAlgorithm));
            HashAlgorithm = hashAlgorithm;
            _hashAlgorithm = new HashAlgorithmFactory().Create(hashAlgorithm);
            _rsaForEncrypt = new RSACryptoServiceProvider();
            _rsaForEncrypt.ImportParameters(publicParameters);
        }

        public RSASignatureAlgorithm(HashAlgorithm hashAlgorithm, RSAParameters publicParameters, RSAParameters privateParameters) : this(hashAlgorithm, publicParameters) {
            _rsaForSign = new RSACryptoServiceProvider();
            _rsaForSign.ImportParameters(privateParameters);
        }

        public HashAlgorithm HashAlgorithm { get; }
        
        public void Dispose() {
            _rsaForEncrypt?.Dispose();
            _rsaForSign?.Dispose();
        }

        public string Name => "RSA";

        public byte[] ComputeHash(string contentToSign) {
            var inputBytes = Encoding.UTF8.GetBytes(contentToSign);
            var hashedData = _hashAlgorithm.ComputeHash(inputBytes);
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