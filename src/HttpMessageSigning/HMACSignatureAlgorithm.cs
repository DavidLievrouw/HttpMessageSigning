using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Dalion.HttpMessageSigning {
    /// <summary>
    /// Represents an HMAC algorithm that is used to sign a request, or to verify a signature.
    /// </summary>
    public class HMACSignatureAlgorithm : ISignatureAlgorithm {
        public HMACSignatureAlgorithm(string secret, HashAlgorithmName hashAlgorithm) {
            if (secret == null) throw new ArgumentNullException(nameof(secret));
            HashAlgorithm = hashAlgorithm;
            Key = Encoding.UTF8.GetBytes(secret);
        }

        public byte[] Key { get; }

        public string Name => "HMAC";

        public HashAlgorithmName HashAlgorithm { get; }

        public byte[] ComputeHash(string contentToSign) {
            var inputBytes = Encoding.UTF8.GetBytes(contentToSign);
            using (var hasher = CreateHMAC(HashAlgorithm, Key)) {
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

        private static readonly IDictionary<HashAlgorithmName, Func<byte[], HMAC>> HMACCreators = new Dictionary<HashAlgorithmName, Func<byte[], HMAC>> {
            { HashAlgorithmName.MD5, key => new HMACMD5(key)},
            { HashAlgorithmName.SHA1, key => new HMACSHA1(key)},
            { HashAlgorithmName.SHA256, key => new HMACSHA256(key)},
            { HashAlgorithmName.SHA384, key => new HMACSHA384(key)},
            { HashAlgorithmName.SHA512, key => new HMACSHA512(key)},
        };
        
        private static HMAC CreateHMAC(HashAlgorithmName hashAlgorithmName, byte[] key) {
            if (!HMACCreators.TryGetValue(hashAlgorithmName, out var creatorFunc)) {
                var fallback = HMAC.Create($"HMAC{hashAlgorithmName.Name}");
                fallback.Key = key;
                return fallback;
            }

            return creatorFunc(key);
        }
    }
}