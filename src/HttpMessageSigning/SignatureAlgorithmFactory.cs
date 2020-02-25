using System;

namespace Dalion.HttpMessageSigning {
    internal class SignatureAlgorithmFactory : ISignatureAlgorithmFactory {
        private readonly IHashAlgorithmFactory _hashAlgorithmFactory;

        public SignatureAlgorithmFactory(IHashAlgorithmFactory hashAlgorithmFactory) {
            _hashAlgorithmFactory = hashAlgorithmFactory ?? throw new ArgumentNullException(nameof(hashAlgorithmFactory));
        }

        public ISignatureAlgorithm Create(SignatureAlgorithm signatureAlgorithm, HashAlgorithm hashAlgorithm, string secret) {
            if (string.IsNullOrEmpty(secret)) throw new ArgumentException("Value cannot be null or empty.", nameof(secret));

            switch (signatureAlgorithm) {
                case SignatureAlgorithm.HMAC:
                    return new HMACSignatureAlgorithm(hashAlgorithm, secret);
                case SignatureAlgorithm.RSA:
                    var hash = _hashAlgorithmFactory.Create(hashAlgorithm);
                    //return new RSASignatureAlgorithm(secret, hash);
                    throw new NotSupportedException();
                default:
                    throw new ArgumentOutOfRangeException(nameof(signatureAlgorithm), signatureAlgorithm, null);
            }
        }
    }
}