using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Dalion.HttpMessageSigning {
    /// <summary>
    ///     Contains static utility factory methods for <see cref="ISignatureAlgorithm" /> implementations.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class SignatureAlgorithm {
        /// <summary>
        ///     Creates a new RSA <see cref="ISignatureAlgorithm" />, for HTTP request signing purposes.
        /// </summary>
        /// <param name="rsa">The RSA algorithm to use.</param>
        /// <returns>The newly created <see cref="ISignatureAlgorithm" />.</returns>
        public static ISignatureAlgorithm CreateForSigning(RSA rsa) {
            if (rsa == null) throw new ArgumentNullException(nameof(rsa));
            return CreateForSigning(rsa, HashAlgorithmName.SHA512);
        }

        /// <summary>
        ///     Creates a new ECDsa <see cref="ISignatureAlgorithm" />, for HTTP request signing purposes.
        /// </summary>
        /// <param name="ecdsa">The ECDsa algorithm to use.</param>
        /// <returns>The newly created <see cref="ISignatureAlgorithm" />.</returns>
        public static ISignatureAlgorithm CreateForSigning(ECDsa ecdsa) {
            if (ecdsa == null) throw new ArgumentNullException(nameof(ecdsa));
            return CreateForSigning(ecdsa, HashAlgorithmName.SHA512);
        }

        /// <summary>
        ///     Creates a new RSA <see cref="ISignatureAlgorithm" />, for HTTP request signing purposes.
        /// </summary>
        /// <param name="privateParameters">The private RSA algorithm parameters to use.</param>
        /// <returns>The newly created <see cref="ISignatureAlgorithm" />.</returns>
        public static ISignatureAlgorithm CreateForSigning(RSAParameters privateParameters) {
            return CreateForSigning(privateParameters, HashAlgorithmName.SHA512);
        }

        /// <summary>
        ///     Creates a new ECDsa <see cref="ISignatureAlgorithm" />, for HTTP request signing purposes.
        /// </summary>
        /// <param name="privateParameters">The private ECDsa algorithm parameters to use.</param>
        /// <returns>The newly created <see cref="ISignatureAlgorithm" />.</returns>
        public static ISignatureAlgorithm CreateForSigning(ECParameters privateParameters) {
            return CreateForSigning(privateParameters, HashAlgorithmName.SHA512);
        }

        /// <summary>
        ///     Creates a new RSA <see cref="ISignatureAlgorithm" />, for HTTP request signing purposes.
        /// </summary>
        /// <param name="certificate">The X.509 certificate to use.</param>
        /// <returns>The newly created <see cref="ISignatureAlgorithm" />.</returns>
        public static ISignatureAlgorithm CreateForSigning(X509Certificate2 certificate) {
            if (certificate == null) throw new ArgumentNullException(nameof(certificate));
            return RSASignatureAlgorithm.CreateForSigning(HashAlgorithmName.SHA512, certificate);
        }

        /// <summary>
        ///     Creates a new HMAC <see cref="ISignatureAlgorithm" />, for HTTP request signing purposes.
        /// </summary>
        /// <param name="hmacSecret">The shared HMAC secret to use.</param>
        /// <returns>The newly created <see cref="ISignatureAlgorithm" />.</returns>
        public static ISignatureAlgorithm CreateForSigning(string hmacSecret) {
            return CreateForSigning(hmacSecret, HashAlgorithmName.SHA512);
        }

        /// <summary>
        ///     Creates a new RSA <see cref="ISignatureAlgorithm" />, for HTTP request signing purposes.
        /// </summary>
        /// <param name="rsa">The RSA algorithm to use.</param>
        /// <param name="hashAlgorithm">The name of the hash algorithm to use.</param>
        /// <returns>The newly created <see cref="ISignatureAlgorithm" />.</returns>
        public static ISignatureAlgorithm CreateForSigning(RSA rsa, HashAlgorithmName hashAlgorithm) {
            if (rsa == null) throw new ArgumentNullException(nameof(rsa));

            return new RSASignatureAlgorithm(hashAlgorithm, rsa);
        }

        /// <summary>
        ///     Creates a new ECDsa <see cref="ISignatureAlgorithm" />, for HTTP request signing purposes.
        /// </summary>
        /// <param name="ecdsa">The ECDsa algorithm to use.</param>
        /// <param name="hashAlgorithm">The name of the hash algorithm to use.</param>
        /// <returns>The newly created <see cref="ISignatureAlgorithm" />.</returns>
        public static ISignatureAlgorithm CreateForSigning(ECDsa ecdsa, HashAlgorithmName hashAlgorithm) {
            if (ecdsa == null) throw new ArgumentNullException(nameof(ecdsa));

            return new ECDsaSignatureAlgorithm(hashAlgorithm, ecdsa);
        }

        /// <summary>
        ///     Creates a new RSA <see cref="ISignatureAlgorithm" />, for HTTP request signing purposes.
        /// </summary>
        /// <param name="privateParameters">The private RSA algorithm parameters to use.</param>
        /// <param name="hashAlgorithm">The name of the hash algorithm to use.</param>
        /// <returns>The newly created <see cref="ISignatureAlgorithm" />.</returns>
        public static ISignatureAlgorithm CreateForSigning(RSAParameters privateParameters, HashAlgorithmName hashAlgorithm) {
            return RSASignatureAlgorithm.CreateForSigning(hashAlgorithm, privateParameters);
        }

        /// <summary>
        ///     Creates a new ECDsa <see cref="ISignatureAlgorithm" />, for HTTP request signing purposes.
        /// </summary>
        /// <param name="privateParameters">The private ECDsa algorithm parameters to use.</param>
        /// <param name="hashAlgorithm">The name of the hash algorithm to use.</param>
        /// <returns>The newly created <see cref="ISignatureAlgorithm" />.</returns>
        public static ISignatureAlgorithm CreateForSigning(ECParameters privateParameters, HashAlgorithmName hashAlgorithm) {
            return ECDsaSignatureAlgorithm.CreateForSigning(hashAlgorithm, privateParameters);
        }

        /// <summary>
        ///     Creates a new RSA <see cref="ISignatureAlgorithm" />, for HTTP request signing purposes.
        /// </summary>
        /// <param name="certificate">The X.509 certificate to use.</param>
        /// <param name="hashAlgorithm">The name of the hash algorithm to use.</param>
        /// <returns>The newly created <see cref="ISignatureAlgorithm" />.</returns>
        public static ISignatureAlgorithm CreateForSigning(X509Certificate2 certificate, HashAlgorithmName hashAlgorithm) {
            if (certificate == null) throw new ArgumentNullException(nameof(certificate));
            return RSASignatureAlgorithm.CreateForSigning(hashAlgorithm, certificate);
        }

        /// <summary>
        ///     Creates a new HMAC <see cref="ISignatureAlgorithm" />, for HTTP request signing purposes.
        /// </summary>
        /// <param name="hmacSecret">The shared HMAC secret to use.</param>
        /// <param name="hashAlgorithm">The name of the hash algorithm to use.</param>
        /// <returns>The newly created <see cref="ISignatureAlgorithm" />.</returns>
        public static ISignatureAlgorithm CreateForSigning(string hmacSecret, HashAlgorithmName hashAlgorithm) {
            if (hmacSecret == null) throw new ArgumentNullException(nameof(hmacSecret));

            return new HMACSignatureAlgorithm(hmacSecret, hashAlgorithm);
        }

        /// <summary>
        ///     Creates a new RSA <see cref="ISignatureAlgorithm" />, for HTTP request signature verification purposes.
        /// </summary>
        /// <param name="rsa">The RSA algorithm to use.</param>
        /// <returns>The newly created <see cref="ISignatureAlgorithm" />.</returns>
        public static ISignatureAlgorithm CreateForVerification(RSA rsa) {
            if (rsa == null) throw new ArgumentNullException(nameof(rsa));
            return CreateForVerification(rsa, HashAlgorithmName.SHA512);
        }

        /// <summary>
        ///     Creates a new ECDsa <see cref="ISignatureAlgorithm" />, for HTTP request signature verification purposes.
        /// </summary>
        /// <param name="ecdsa">The ECDsa algorithm to use.</param>
        /// <returns>The newly created <see cref="ISignatureAlgorithm" />.</returns>
        public static ISignatureAlgorithm CreateForVerification(ECDsa ecdsa) {
            if (ecdsa == null) throw new ArgumentNullException(nameof(ecdsa));
            return CreateForVerification(ecdsa, HashAlgorithmName.SHA512);
        }

        /// <summary>
        ///     Creates a new ECDsa <see cref="ISignatureAlgorithm" />, for HTTP request signature verification purposes.
        /// </summary>
        /// <param name="publicParameters">The public RSA algorithm parameters to use.</param>
        /// <returns>The newly created <see cref="ISignatureAlgorithm" />.</returns>
        public static ISignatureAlgorithm CreateForVerification(RSAParameters publicParameters) {
            return CreateForVerification(publicParameters, HashAlgorithmName.SHA512);
        }

        /// <summary>
        ///     Creates a new ECDsa <see cref="ISignatureAlgorithm" />, for HTTP request signature verification purposes.
        /// </summary>
        /// <param name="publicParameters">The public ECDsa algorithm parameters to use.</param>
        /// <returns>The newly created <see cref="ISignatureAlgorithm" />.</returns>
        public static ISignatureAlgorithm CreateForVerification(ECParameters publicParameters) {
            return CreateForVerification(publicParameters, HashAlgorithmName.SHA512);
        }

        /// <summary>
        ///     Creates a new RSA <see cref="ISignatureAlgorithm" />, for HTTP request signature verification purposes.
        /// </summary>
        /// <param name="certificate">The X.509 certificate to use.</param>
        /// <returns>The newly created <see cref="ISignatureAlgorithm" />.</returns>
        public static ISignatureAlgorithm CreateForVerification(X509Certificate2 certificate) {
            return CreateForVerification(certificate, HashAlgorithmName.SHA512);
        }

        /// <summary>
        ///     Creates a new HMAC <see cref="ISignatureAlgorithm" />, for HTTP request signature verification purposes.
        /// </summary>
        /// <param name="hmacSecret">The shared HMAC secret to use.</param>
        /// <returns>The newly created <see cref="ISignatureAlgorithm" />.</returns>
        public static ISignatureAlgorithm CreateForVerification(string hmacSecret) {
            return CreateForVerification(hmacSecret, HashAlgorithmName.SHA512);
        }

        /// <summary>
        ///     Creates a new RSA <see cref="ISignatureAlgorithm" />, for HTTP request signature verification purposes.
        /// </summary>
        /// <param name="rsa">The RSA algorithm to use.</param>
        /// <param name="hashAlgorithm">The name of the hash algorithm to use.</param>
        /// <returns>The newly created <see cref="ISignatureAlgorithm" />.</returns>
        public static ISignatureAlgorithm CreateForVerification(RSA rsa, HashAlgorithmName hashAlgorithm) {
            if (rsa == null) throw new ArgumentNullException(nameof(rsa));

            return new RSASignatureAlgorithm(hashAlgorithm, rsa);
        }

        /// <summary>
        ///     Creates a new ECDsa <see cref="ISignatureAlgorithm" />, for HTTP request signature verification purposes.
        /// </summary>
        /// <param name="ecdsa">The ECDsa algorithm to use.</param>
        /// <param name="hashAlgorithm">The name of the hash algorithm to use.</param>
        /// <returns>The newly created <see cref="ISignatureAlgorithm" />.</returns>
        public static ISignatureAlgorithm CreateForVerification(ECDsa ecdsa, HashAlgorithmName hashAlgorithm) {
            if (ecdsa == null) throw new ArgumentNullException(nameof(ecdsa));

            return new ECDsaSignatureAlgorithm(hashAlgorithm, ecdsa);
        }

        /// <summary>
        ///     Creates a new RSA <see cref="ISignatureAlgorithm" />, for HTTP request signature verification purposes.
        /// </summary>
        /// <param name="publicParameters">The public RSA algorithm parameters to use.</param>
        /// <param name="hashAlgorithm">The name of the hash algorithm to use.</param>
        /// <returns>The newly created <see cref="ISignatureAlgorithm" />.</returns>
        public static ISignatureAlgorithm CreateForVerification(RSAParameters publicParameters, HashAlgorithmName hashAlgorithm) {
            return RSASignatureAlgorithm.CreateForVerification(hashAlgorithm, publicParameters);
        }

        /// <summary>
        ///     Creates a new ECDsa <see cref="ISignatureAlgorithm" />, for HTTP request signature verification purposes.
        /// </summary>
        /// <param name="publicParameters">The public ECDsa algorithm parameters to use.</param>
        /// <param name="hashAlgorithm">The name of the hash algorithm to use.</param>
        /// <returns>The newly created <see cref="ISignatureAlgorithm" />.</returns>
        public static ISignatureAlgorithm CreateForVerification(ECParameters publicParameters, HashAlgorithmName hashAlgorithm) {
            return ECDsaSignatureAlgorithm.CreateForVerification(hashAlgorithm, publicParameters);
        }

        /// <summary>
        ///     Creates a new RSA <see cref="ISignatureAlgorithm" />, for HTTP request signature verification purposes.
        /// </summary>
        /// <param name="certificate">The X.509 certificate to use.</param>
        /// <param name="hashAlgorithm">The name of the hash algorithm to use.</param>
        /// <returns>The newly created <see cref="ISignatureAlgorithm" />.</returns>
        public static ISignatureAlgorithm CreateForVerification(X509Certificate2 certificate, HashAlgorithmName hashAlgorithm) {
            if (certificate == null) throw new ArgumentNullException(nameof(certificate));
            var publicKey = certificate.GetRSAPublicKey();
            var publicParameters = publicKey.ExportParameters(false);

            return CreateForVerification(publicParameters, hashAlgorithm);
        }

        /// <summary>
        ///     Creates a new HMAC <see cref="ISignatureAlgorithm" />, for HTTP request signature verification purposes.
        /// </summary>
        /// <param name="hmacSecret">The shared HMAC secret to use.</param>
        /// <param name="hashAlgorithm">The name of the hash algorithm to use.</param>
        /// <returns>The newly created <see cref="ISignatureAlgorithm" />.</returns>
        public static ISignatureAlgorithm CreateForVerification(string hmacSecret, HashAlgorithmName hashAlgorithm) {
            if (hmacSecret == null) throw new ArgumentNullException(nameof(hmacSecret));

            return new HMACSignatureAlgorithm(hmacSecret, hashAlgorithm);
        }
    }
}