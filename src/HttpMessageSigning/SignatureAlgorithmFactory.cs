using System;

namespace Dalion.HttpMessageSigning {
    internal class SignatureAlgorithmFactory : ISignatureAlgorithmFactory {
        public ISignatureAlgorithm Create(Secret secret, HashAlgorithm hashAlgorithm) {
            if (secret == null) throw new ArgumentNullException(nameof(secret));
            
            switch (secret) {
                case HMACSecret hmacSecret:
                    return new HMACSignatureAlgorithm(hashAlgorithm, hmacSecret);
                /*case SignatureAlgorithm.RSA:
                    var hash = _hashAlgorithmFactory.Create(hashAlgorithm);
                    //return new RSASignatureAlgorithm(secret, hash);
                    throw new NotSupportedException();*/
                default:
                    throw new NotSupportedException($"The specified secret type ({secret.GetType().Name}) is currently not supported.");
            }
        }
    }
}