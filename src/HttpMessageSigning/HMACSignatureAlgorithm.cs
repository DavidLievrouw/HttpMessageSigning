using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.ObjectPool;

namespace Dalion.HttpMessageSigning {
    /// <summary>
    ///     Represents an HMAC algorithm that is used to sign a request, or to verify a signature.
    /// </summary>
    public class HMACSignatureAlgorithm : ISignatureAlgorithm {
        private static readonly IDictionary<HashAlgorithmName, Func<byte[], HMAC>> HMACCreators = new Dictionary<HashAlgorithmName, Func<byte[], HMAC>> {
            {HashAlgorithmName.MD5, key => new HMACMD5(key)},
            {HashAlgorithmName.SHA1, key => new HMACSHA1(key)},
            {HashAlgorithmName.SHA256, key => new HMACSHA256(key)},
            {HashAlgorithmName.SHA384, key => new HMACSHA384(key)},
            {HashAlgorithmName.SHA512, key => new HMACSHA512(key)},
        };

        private static readonly ObjectPoolProvider PoolProvider = new DefaultObjectPoolProvider {MaximumRetained = Environment.ProcessorCount * 3};
        
        private readonly ObjectPool<HMAC> _hasherPool; 
        
        /// <summary>
        ///     Creates a new <see cref="HMACSignatureAlgorithm" />.
        /// </summary>
        /// <param name="secret">The shared key of the HMAC algorithm.</param>
        /// <param name="hashAlgorithm">The name of the hash algorithm to use.</param>
        public HMACSignatureAlgorithm(string secret, HashAlgorithmName hashAlgorithm) {
            if (secret == null) throw new ArgumentNullException(nameof(secret));
            HashAlgorithm = hashAlgorithm;
            Key = Encoding.UTF8.GetBytes(secret);
            _hasherPool = PoolProvider.Create(new PooledHMACPolicy(() => CreateHMAC(HashAlgorithm, Key)));
        }

        /// <summary>
        ///     Gets the shared key of the HMAC algorithm.
        /// </summary>
        public byte[] Key { get; }

        /// <inheritdoc />
        public string Name => "HMAC";

        /// <inheritdoc />
        public HashAlgorithmName HashAlgorithm { get; }

        /// <inheritdoc />
        public byte[] ComputeHash(string contentToSign) {
            var inputBytes = Encoding.UTF8.GetBytes(contentToSign);

            HMAC hasher = null;
            try {
                hasher = _hasherPool.Get();
                return hasher.ComputeHash(inputBytes);
            }
            finally {
                if (hasher != null) _hasherPool.Return(hasher);
            }
        }

        /// <inheritdoc />
        public bool VerifySignature(string contentToSign, byte[] signature) {
            if (contentToSign == null) throw new ArgumentNullException(nameof(contentToSign));
            if (signature == null) throw new ArgumentNullException(nameof(signature));

            var computed = ComputeHash(contentToSign);
            return signature.SequenceEqual(computed);
        }

        /// <inheritdoc />
        public void Dispose() {
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (_hasherPool is IDisposable disposable) {
                disposable.Dispose();
            }
        }

        private static HMAC CreateHMAC(HashAlgorithmName hashAlgorithmName, byte[] key) {
            if (!HMACCreators.TryGetValue(hashAlgorithmName, out var creatorFunc)) {
                var fallback = HMAC.Create($"HMAC{hashAlgorithmName.Name}");
                if (fallback == null) throw new NotSupportedException($"The specified hash algorithm '{hashAlgorithmName.Name}' is not supported.");
                fallback.Key = key;
                return fallback;
            }

            return creatorFunc(key);
        }
    }
}