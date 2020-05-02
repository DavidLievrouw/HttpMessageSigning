using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Dalion.HttpMessageSigning {
    /// <summary>
    /// Contains static utility factory methods for <see cref="ISignatureAlgorithm"/> implementations.
    /// </summary>
    public static class SignatureAlgorithm {
        public static ISignatureAlgorithm CreateForSigning(RSA rsa) {
            if (rsa == null) throw new ArgumentNullException(nameof(rsa));
            return CreateForSigning(rsa, HashAlgorithmName.SHA256);
        }

        public static ISignatureAlgorithm CreateForSigning(RSAParameters privateParameters) {
            return CreateForSigning(privateParameters, HashAlgorithmName.SHA256);
        }

        public static ISignatureAlgorithm CreateForSigning(X509Certificate2 certificate) {
            return CreateForSigning(certificate, HashAlgorithmName.SHA256);
        }

        public static ISignatureAlgorithm CreateForSigning(string hmacSecret) {
            return CreateForSigning(hmacSecret, HashAlgorithmName.SHA256);
        }

        public static ISignatureAlgorithm CreateForSigning(RSA rsa, HashAlgorithmName hashAlgorithm) {
            if (rsa == null) throw new ArgumentNullException(nameof(rsa));

            return new RSASignatureAlgorithm(hashAlgorithm, rsa);
        }

        public static ISignatureAlgorithm CreateForSigning(RSAParameters privateParameters, HashAlgorithmName hashAlgorithm) {
            return RSASignatureAlgorithm.CreateForSigning(hashAlgorithm, privateParameters);
        }

        public static ISignatureAlgorithm CreateForSigning(X509Certificate2 certificate, HashAlgorithmName hashAlgorithm) {
            if (certificate == null) throw new ArgumentNullException(nameof(certificate));
            var privateKey = certificate.GetRSAPrivateKey();
            var privateParameters = privateKey.ExportParameters(true);

            return CreateForSigning(privateParameters, hashAlgorithm);
        }

        public static ISignatureAlgorithm CreateForSigning(string hmacSecret, HashAlgorithmName hashAlgorithm) {
            if (hmacSecret == null) throw new ArgumentNullException(nameof(hmacSecret));

            return new HMACSignatureAlgorithm(hmacSecret, hashAlgorithm);
        }
        
        public static ISignatureAlgorithm CreateForVerification(RSA rsa) {
            if (rsa == null) throw new ArgumentNullException(nameof(rsa));
            return CreateForVerification(rsa, HashAlgorithmName.SHA256);
        }

        public static ISignatureAlgorithm CreateForVerification(RSAParameters publicParameters) {
            return CreateForVerification(publicParameters, HashAlgorithmName.SHA256);
        }

        public static ISignatureAlgorithm CreateForVerification(X509Certificate2 certificate) {
            return CreateForVerification(certificate, HashAlgorithmName.SHA256);
        }

        public static ISignatureAlgorithm CreateForVerification(string hmacSecret) {
            return CreateForVerification(hmacSecret, HashAlgorithmName.SHA256);
        }

        public static ISignatureAlgorithm CreateForVerification(RSA rsa, HashAlgorithmName hashAlgorithm) {
            if (rsa == null) throw new ArgumentNullException(nameof(rsa));

            return new RSASignatureAlgorithm(hashAlgorithm, rsa);
        }

        public static ISignatureAlgorithm CreateForVerification(RSAParameters publicParameters, HashAlgorithmName hashAlgorithm) {
            return RSASignatureAlgorithm.CreateForVerification(hashAlgorithm, publicParameters);
        }

        public static ISignatureAlgorithm CreateForVerification(X509Certificate2 certificate, HashAlgorithmName hashAlgorithm) {
            if (certificate == null) throw new ArgumentNullException(nameof(certificate));
            var publicKey = certificate.GetRSAPublicKey();
            var publicParameters = publicKey.ExportParameters(false);

            return CreateForVerification(publicParameters, hashAlgorithm);
        }

        public static ISignatureAlgorithm CreateForVerification(string hmacSecret, HashAlgorithmName hashAlgorithm) {
            if (hmacSecret == null) throw new ArgumentNullException(nameof(hmacSecret));

            return new HMACSignatureAlgorithm(hmacSecret, hashAlgorithm);
        }
    }
}