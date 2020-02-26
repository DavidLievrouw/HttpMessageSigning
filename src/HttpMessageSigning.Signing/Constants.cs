using System.Collections.Generic;
using System.Security.Cryptography;

namespace Dalion.HttpMessageSigning.Signing {
    internal static class Constants {
        public static IDictionary<string, string> DigestHashAlgorithmNames = new Dictionary<string, string> {
            { HashAlgorithmName.SHA1.Name, "SHA-1" },
            { HashAlgorithmName.SHA256.Name, "SHA-256" },
            { HashAlgorithmName.SHA384.Name, "SHA-384" },
            { HashAlgorithmName.SHA512.Name, "SHA-512" },
            { HashAlgorithmName.MD5.Name, "MD5" }
        };
    }
}