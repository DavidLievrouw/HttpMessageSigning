using System;
using System.Security.Cryptography;
using System.Text;

namespace Dalion.HttpMessageSigning {
    internal class KeyedHashAlgorithmFactory : IKeyedHashAlgorithmFactory {
        public IKeyedHashAlgorithm Create(SignatureAlgorithm signatureAlgorithm, HashAlgorithm hashAlgorithm, string secret) {
            if (signatureAlgorithm != SignatureAlgorithm.HMAC) throw new NotSupportedException($"The signature algorithm '{signatureAlgorithm}' is not supported in this version.");
            if (string.IsNullOrEmpty(secret)) throw new ArgumentException("Value cannot be null or empty.", nameof(secret));

            var algorithmName = $"HMAC{hashAlgorithm}";
            var algorithm = HMAC.Create(algorithmName);
            
            algorithm.Key = Encoding.UTF8.GetBytes(secret);
            
            return new RealKeyedHashAlgorithm(algorithmName, algorithm);
        }
    }
}