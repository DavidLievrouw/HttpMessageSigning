using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Dalion.HttpMessageSigning {
    internal class HMACSignatureAlgorithm : ISignatureAlgorithm {
        private readonly KeyedHashAlgorithm _realAlgorithm;
        
        public HMACSignatureAlgorithm(HashAlgorithm hashAlgorithm, string secret) {
            if (secret == null) throw new ArgumentNullException(nameof(secret));
            var algorithmName = $"HMAC{hashAlgorithm}";
            _realAlgorithm = HMAC.Create(algorithmName);
            _realAlgorithm.Key = Encoding.UTF8.GetBytes(secret);
        }

        public byte[] ComputeHash(string contentToSign) {
            var inputBytes = Encoding.UTF8.GetBytes(contentToSign);
            return _realAlgorithm.ComputeHash(inputBytes);
        }

        public bool VerifySignature(string contentToSign, byte[] signature) {
            if (contentToSign == null) throw new ArgumentNullException(nameof(contentToSign));
            if (signature == null) throw new ArgumentNullException(nameof(signature));
            
            var computed = ComputeHash(contentToSign);
            return signature.SequenceEqual(computed);
        }

        public void Dispose() {
            _realAlgorithm?.Dispose();
        }
    }
}