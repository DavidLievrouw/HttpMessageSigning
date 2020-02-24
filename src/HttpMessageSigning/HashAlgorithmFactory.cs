using System;
using System.Security.Cryptography;

namespace Dalion.HttpMessageSigning {
    internal class HashAlgorithmFactory : IHashAlgorithmFactory {
        public IHashAlgorithm Create(HashAlgorithm hashAlgorithm) {
            switch (hashAlgorithm) {
                case HashAlgorithm.None:
                    throw new ArgumentOutOfRangeException(nameof(hashAlgorithm), hashAlgorithm, $"Cannot create a hash algorithm for value '{hashAlgorithm}'.");
                case HashAlgorithm.SHA1:
                    return new RealHashAlgorithm("SHA-1", SHA1.Create());
                case HashAlgorithm.SHA256:
                    return new RealHashAlgorithm("SHA-256", SHA256.Create());
                case HashAlgorithm.SHA384:
                    return new RealHashAlgorithm("SHA-384", SHA384.Create());
                case HashAlgorithm.SHA512:
                    return new RealHashAlgorithm("SHA-512", SHA512.Create());
                default:
                    throw new ArgumentOutOfRangeException(nameof(hashAlgorithm), hashAlgorithm, "The specified hash algorithm is not supported in this version.");
            }
        }
    }
}