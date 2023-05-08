using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.Extensions.ObjectPool;

namespace Dalion.HttpMessageSigning {
    /// <summary>
    ///     Represents the ECDsa signature algorithm.
    /// </summary>
    public class ECDsaSignatureAlgorithm : ISignatureAlgorithm {
        private static readonly ObjectPoolProvider PoolProvider = new DefaultObjectPoolProvider {MaximumRetained = Environment.ProcessorCount * 3};
        
        private readonly ECDsa _ecdsa;
        private readonly ObjectPool<HashAlgorithm> _hasherPool;

        /// <summary>
        ///     Creates a new <see cref="ECDsaSignatureAlgorithm" />.
        /// </summary>
        /// <param name="hashAlgorithm">The name of the hash algorithm to use.</param>
        /// <param name="ecdsa">The Elliptic Curve Digital Signature Algorithm.</param>
        public ECDsaSignatureAlgorithm(HashAlgorithmName hashAlgorithm, ECDsa ecdsa) {
            HashAlgorithm = hashAlgorithm;
            _ecdsa = ecdsa ?? throw new ArgumentNullException(nameof(ecdsa));
            _hasherPool = PoolProvider.Create(new PooledHashAlgorithmPolicy(() => HashAlgorithmFactory.Create(HashAlgorithm)));
        }

        /// <inheritdoc />
        public HashAlgorithmName HashAlgorithm { get; }

        /// <inheritdoc />
        public void Dispose() {
            _ecdsa?.Dispose();
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (_hasherPool is IDisposable disposable) {
                disposable.Dispose();
            }
        }

        /// <inheritdoc />
        public string Name => "ECDsa";

        /// <inheritdoc />
        public byte[] ComputeHash(string contentToSign) {
            var inputBytes = Encoding.UTF8.GetBytes(contentToSign);
            
            HashAlgorithm hasher = null;
            try {
                hasher = _hasherPool.Get();
                var hashedData = hasher.ComputeHash(inputBytes);
                return _ecdsa.SignHash(hashedData);
            }
            finally {
                if (hasher != null) _hasherPool.Return(hasher);
            }
        }

        /// <inheritdoc />
        public bool VerifySignature(string contentToSign, byte[] signature) {
            if (contentToSign == null) throw new ArgumentNullException(nameof(contentToSign));
            if (signature == null) throw new ArgumentNullException(nameof(signature));

            var signedBytes = Encoding.UTF8.GetBytes(contentToSign);

            HashAlgorithm hasher = null;
            try {
                hasher = _hasherPool.Get();
                var hashedData = hasher.ComputeHash(signedBytes);
                return _ecdsa.VerifyHash(hashedData, signature);
            }
            finally {
                if (hasher != null) _hasherPool.Return(hasher);
            }
        }

        /// <summary>
        ///     Create a new instance of this class, using the specified settings, for signing purposes.
        /// </summary>
        /// <param name="hashAlgorithmName">The name of the hash algorithm to use.</param>
        /// <param name="privateParameters">The private parameters for the ECDsa algorithm.</param>
        /// <returns>A new <see cref="ECDsaSignatureAlgorithm" />.</returns>
        public static ECDsaSignatureAlgorithm CreateForSigning(HashAlgorithmName hashAlgorithmName, ECParameters privateParameters) {
            var ecdsa = ECDsa.Create();
            ecdsa.ImportParameters(privateParameters);
            return new ECDsaSignatureAlgorithm(hashAlgorithmName, ecdsa);
        }

        /// <summary>
        ///     Create a new instance of this class, using the specified settings, for signing purposes.
        /// </summary>
        /// <param name="hashAlgorithmName">The name of the hash algorithm to use.</param>
        /// <param name="cert">The certificate with the private key for the ECDsa algorithm.</param>
        /// <returns>A new <see cref="ECDsaSignatureAlgorithm" />.</returns>
        public static ECDsaSignatureAlgorithm CreateForSigning(HashAlgorithmName hashAlgorithmName, X509Certificate2 cert) {
            if (cert == null) throw new ArgumentNullException(nameof(cert));
            return new ECDsaSignatureAlgorithm(hashAlgorithmName, cert.GetECDsaPrivateKey());
        }

        /// <summary>
        ///     Create a new instance of this class, using the specified settings, for verification purposes.
        /// </summary>
        /// <param name="hashAlgorithmName">The name of the hash algorithm to use.</param>
        /// <param name="publicParameters">The public parameters for the ECDsa algorithm.</param>
        /// <returns>A new <see cref="ECDsaSignatureAlgorithm" />.</returns>
        public static ECDsaSignatureAlgorithm CreateForVerification(HashAlgorithmName hashAlgorithmName, ECParameters publicParameters) {
            var ecdsa = ECDsa.Create();
            ecdsa.ImportParameters(publicParameters);
            return new ECDsaSignatureAlgorithm(hashAlgorithmName, ecdsa);
        }

        /// <summary>
        ///     Export the public key of this ECDsa algorithm.
        /// </summary>
        /// <returns>The parameters containing the public key of this ECDsa algorithm.</returns>
        public ECParameters GetPublicKey() {
            return _ecdsa.ExportParameters(false);
        }
    }
}