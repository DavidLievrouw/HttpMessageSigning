using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Dalion.HttpMessageSigning.Verification {
    internal static class Constants {
        internal static IDictionary<string, string> DigestHashAlgorithms = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
            { "SHA-1", HashAlgorithmName.SHA1.Name },
            { "SHA-256", HashAlgorithmName.SHA256.Name },
            { "SHA-384", HashAlgorithmName.SHA384.Name },
            { "SHA-512", HashAlgorithmName.SHA512.Name },
            { "SHA-MD5", HashAlgorithmName.MD5.Name }
        };
    }
}