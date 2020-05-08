using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Dalion.HttpMessageSigning {
    /// <summary>
    /// Represents an HMAC algorithm that is used to sign a request, or to verify a signature.
    /// </summary>
    public class HMACSignatureAlgorithm : ISignatureAlgorithm {
        private readonly string _algorithmName;

        public HMACSignatureAlgorithm(string secret, HashAlgorithmName hashAlgorithm) {
            Secret = secret ?? throw new ArgumentNullException(nameof(secret));
            _algorithmName = $"HMAC{hashAlgorithm}";
            HashAlgorithm = hashAlgorithm;
        }

        public string Secret { get; }

        public string Name => "HMAC";

        public HashAlgorithmName HashAlgorithm { get; }

        public byte[] ComputeHash(string contentToSign) {
            var inputBytes = Encoding.UTF8.GetBytes(contentToSign);
            using (var hasher = HMAC.Create(_algorithmName)) {
                hasher.Key = Encoding.UTF8.GetBytes(Secret);
                return hasher.ComputeHash(inputBytes);
            }
        }

        public bool VerifySignature(string contentToVerify, byte[] signature) {
            if (contentToVerify == null) throw new ArgumentNullException(nameof(contentToVerify));
            if (signature == null) throw new ArgumentNullException(nameof(signature));

            var computed = ComputeHash(contentToVerify);
            return signature.SequenceEqual(computed);
        }

        public void Dispose() {
            // Noop
        }
    }
}