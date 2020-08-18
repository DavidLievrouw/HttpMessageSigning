using System;
using System.Security.Cryptography;
using System.Text;

namespace Dalion.HttpMessageSigning {
    /// <summary>
    ///     Represents an RSA algorithm that is used to sign a request, or to verify a signature.
    /// </summary>
    public class RSASignatureAlgorithm : ISignatureAlgorithm {
        private readonly RSA _rsa;

        /// <summary>
        ///     Creates a new <see cref="RSASignatureAlgorithm" />.
        /// </summary>
        /// <param name="rsa">The RSA algorithm.</param>
        /// <param name="hashAlgorithm">The name of the hash algorithm to use.</param>
        public RSASignatureAlgorithm(HashAlgorithmName hashAlgorithm, RSA rsa) {
            HashAlgorithm = hashAlgorithm;
            _rsa = rsa ?? throw new ArgumentNullException(nameof(rsa));
        }

        /// <inheritdoc />
        public HashAlgorithmName HashAlgorithm { get; }

        /// <inheritdoc />
        public void Dispose() {
            _rsa?.Dispose();
        }

        /// <inheritdoc />
        public string Name => "RSA";

        /// <inheritdoc />
        public byte[] ComputeHash(string contentToSign) {
            var inputBytes = Encoding.UTF8.GetBytes(contentToSign);
            using (var hasher = HashAlgorithmFactory.Create(HashAlgorithm)) {
                var hashedData = hasher.ComputeHash(inputBytes);
                return _rsa.SignHash(hashedData, HashAlgorithm, RSASignaturePadding.Pkcs1);
            }
        }

        /// <inheritdoc />
        public bool VerifySignature(string contentToSign, byte[] signature) {
            if (contentToSign == null) throw new ArgumentNullException(nameof(contentToSign));
            if (signature == null) throw new ArgumentNullException(nameof(signature));

            var signedBytes = Encoding.UTF8.GetBytes(contentToSign);
            using (var hasher = HashAlgorithmFactory.Create(HashAlgorithm)) {
                var hashedData = hasher.ComputeHash(signedBytes);
                return _rsa.VerifyHash(hashedData, signature, HashAlgorithm, RSASignaturePadding.Pkcs1);
            }
        }

        /// <summary>
        ///     Create a new instance of this class, using the specified settings, for signing purposes.
        /// </summary>
        /// <param name="hashAlgorithmName">The name of the hash algorithm to use.</param>
        /// <param name="privateParameters">The private parameters for the RSA algorithm.</param>
        /// <returns>A new <see cref="RSASignatureAlgorithm" />.</returns>
        public static RSASignatureAlgorithm CreateForSigning(HashAlgorithmName hashAlgorithmName, RSAParameters privateParameters) {
            var rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(privateParameters);
            return new RSASignatureAlgorithm(hashAlgorithmName, rsa);
        }

        /// <summary>
        ///     Create a new instance of this class, using the specified settings, for verification purposes.
        /// </summary>
        /// <param name="hashAlgorithmName">The name of the hash algorithm to use.</param>
        /// <param name="publicParameters">The public parameters for the RSA algorithm.</param>
        /// <returns>A new <see cref="RSASignatureAlgorithm" />.</returns>
        public static RSASignatureAlgorithm CreateForVerification(HashAlgorithmName hashAlgorithmName, RSAParameters publicParameters) {
            var rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(publicParameters);
            return new RSASignatureAlgorithm(hashAlgorithmName, rsa);
        }

        /// <summary>
        ///     Export the public key of this RSA algorithm.
        /// </summary>
        /// <returns>The parameters containing the public key of this RSA algorithm.</returns>
        public RSAParameters GetPublicKey() {
            return _rsa.ExportParameters(false);
        }
    }
}