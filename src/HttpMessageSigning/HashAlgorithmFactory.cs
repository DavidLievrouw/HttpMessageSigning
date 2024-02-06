using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Dalion.HttpMessageSigning {
    internal static class HashAlgorithmFactory {
        private static readonly IDictionary<HashAlgorithmName, Func<HashAlgorithm>> HashAlgorithmCreators = new Dictionary<HashAlgorithmName, Func<HashAlgorithm>> {
            {HashAlgorithmName.MD5, MD5.Create},
            {HashAlgorithmName.SHA1, SHA1.Create},
            {HashAlgorithmName.SHA256,SHA256.Create},
            {HashAlgorithmName.SHA384,SHA384.Create},
            {HashAlgorithmName.SHA512,SHA512.Create}
        };

        public static HashAlgorithm Create(HashAlgorithmName hashAlgorithmName) {
            if (!HashAlgorithmCreators.TryGetValue(hashAlgorithmName, out var creatorFunc)) {
                var fallback = (HashAlgorithm)CryptoConfig.CreateFromName(hashAlgorithmName.Name);
                if (fallback == null) throw new NotSupportedException($"The specified hash algorithm '{hashAlgorithmName.Name}' is not supported.");
                return fallback;
            }

            return creatorFunc();
        }
    }
}