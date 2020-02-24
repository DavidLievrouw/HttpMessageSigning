using System;
using System.Security.Cryptography;

namespace Dalion.HttpMessageSigning {
    internal class KeyedHashAlgorithmFactory : IKeyedHashAlgorithmFactory {
        public IKeyedHashAlgorithm Create(SignatureAlgorithm signatureAlgorithm, HashAlgorithm hashAlgorithm, byte[] signingKey) {
            if (signatureAlgorithm != SignatureAlgorithm.HMAC) throw new NotSupportedException($"The signature algorithm '{signatureAlgorithm}' is not supported in this version.");

            var algorithmName = $"HMAC{hashAlgorithm}";
            var algorithm = HMAC.Create(algorithmName);
            
            algorithm.Key = signingKey;
            
            return new RealKeyedHashAlgorithm(algorithmName, algorithm);
        }
    }
}