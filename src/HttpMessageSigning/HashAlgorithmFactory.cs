using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Dalion.HttpMessageSigning {
    internal static class HashAlgorithmFactory {
        private static readonly IDictionary<HashAlgorithmName, Func<HashAlgorithm>> HashAlgorithmCreators = new Dictionary<HashAlgorithmName, Func<HashAlgorithm>> {
            {HashAlgorithmName.MD5, () => new MD5CryptoServiceProvider()},
            {HashAlgorithmName.SHA1, () => new SHA1CryptoServiceProvider()},
            {HashAlgorithmName.SHA256, () => new SHA256Managed()},
            {HashAlgorithmName.SHA384, () => new SHA384Managed()},
            {HashAlgorithmName.SHA512, () => new SHA512Managed()}
        };

        public static HashAlgorithm Create(HashAlgorithmName hashAlgorithmName) {
            if (!HashAlgorithmCreators.TryGetValue(hashAlgorithmName, out var creatorFunc)) {
                var fallback =  HashAlgorithm.Create(hashAlgorithmName.Name);
                if (fallback == null) throw new NotSupportedException($"The specified hash algorithm '{hashAlgorithmName.Name}' is not supported.");
                return fallback;
            }

            return creatorFunc();
        }
    }
}