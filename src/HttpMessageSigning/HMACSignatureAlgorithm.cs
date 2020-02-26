using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Dalion.HttpMessageSigning {
    /// <summary>
    /// Represents an HMAC algorithm that is used to sign a request, or to verify a signature.
    /// </summary>
    public class HMACSignatureAlgorithm : ISignatureAlgorithm {
        private readonly KeyedHashAlgorithm _realAlgorithm;
        
        public HMACSignatureAlgorithm(string secret, HashAlgorithmName hashAlgorithm) {
            if (secret == null) throw new ArgumentNullException(nameof(secret));
            var algorithmName = $"HMAC{hashAlgorithm}";
            HashAlgorithm = hashAlgorithm;
            Secret = secret;
            _realAlgorithm = HMAC.Create(algorithmName);
            _realAlgorithm.Key = Encoding.UTF8.GetBytes(secret);
        }

        public string Secret { get; }
        
        public string Name => "HMAC";
        
        public HashAlgorithmName HashAlgorithm { get; }

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