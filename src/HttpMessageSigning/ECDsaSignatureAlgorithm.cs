using System;
using System.Security.Cryptography;
using System.Text;

namespace Dalion.HttpMessageSigning {
    /// <summary>
    ///     Represents the ECDsa signature algorithm.
    /// </summary>
    public class ECDsaSignatureAlgorithm : ISignatureAlgorithm {
        private readonly ECDsa _ecdsa;

        /// <summary>
        ///     Creates a new <see cref="ECDsaSignatureAlgorithm" />.
        /// </summary>
        /// <param name="hashAlgorithm">The name of the hash algorithm to use.</param>
        /// <param name="ecdsa">The Elliptic Curve Digital Signature Algorithm.</param>
        public ECDsaSignatureAlgorithm(HashAlgorithmName hashAlgorithm, ECDsa ecdsa) {
            HashAlgorithm = hashAlgorithm;
            _ecdsa = ecdsa ?? throw new ArgumentNullException(nameof(ecdsa));
        }

        /// <inheritdoc />
        public HashAlgorithmName HashAlgorithm { get; }

        /// <inheritdoc />
        public void Dispose() {
            _ecdsa?.Dispose();
        }

        /// <inheritdoc />
        public string Name => "ECDsa";

        /// <inheritdoc />
        public byte[] ComputeHash(string contentToSign) {
            var inputBytes = Encoding.UTF8.GetBytes(contentToSign);

            using (var hasher = HashAlgorithmFactory.Create(HashAlgorithm)) {
                var hashedData = hasher.ComputeHash(inputBytes);
                return _ecdsa.SignHash(hashedData);
            }
        }

        /// <inheritdoc />
        public bool VerifySignature(string contentToSign, byte[] signature) {
            if (contentToSign == null) throw new ArgumentNullException(nameof(contentToSign));
            if (signature == null) throw new ArgumentNullException(nameof(signature));

            var signedBytes = Encoding.UTF8.GetBytes(contentToSign);

            using (var hasher = HashAlgorithmFactory.Create(HashAlgorithm)) {
                var hashedData = hasher.ComputeHash(signedBytes);
                return _ecdsa.VerifyHash(hashedData, signature);
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