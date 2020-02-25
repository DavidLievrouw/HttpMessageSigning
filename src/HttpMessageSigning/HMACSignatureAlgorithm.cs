using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Dalion.HttpMessageSigning {
    /// <summary>
    /// Represents an HMAC algorithm that is used to sign a request, or to validate a signature.
    /// </summary>
    public class HMACSignatureAlgorithm : ISignatureAlgorithm {
        private readonly KeyedHashAlgorithm _realAlgorithm;
        
        public HMACSignatureAlgorithm(string secret, HashAlgorithm hashAlgorithm) {
            if (secret == null) throw new ArgumentNullException(nameof(secret));
            if (hashAlgorithm == HashAlgorithm.None) throw new ArgumentException("A hash algorithm must be specified.", nameof(hashAlgorithm));
            var algorithmName = $"HMAC{hashAlgorithm}";
            HashAlgorithm = hashAlgorithm;
            Secret = secret;
            _realAlgorithm = HMAC.Create(algorithmName);
            _realAlgorithm.Key = Encoding.UTF8.GetBytes(secret);
        }

        public string Secret { get; }
        
        public string Name => "HMAC";
        
        public HashAlgorithm HashAlgorithm { get; }

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