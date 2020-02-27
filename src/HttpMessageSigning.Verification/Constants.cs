using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Dalion.HttpMessageSigning.Verification {
    public static class Constants {
        public static class ClaimTypes {
            public const string AppId = "appid";
            public const string Role = "role";
        }

        public static class AuthenticationSchemes {
            public const string HttpRequestSignature = "HttpRequestSignature";
        }
        
        public static IDictionary<string, string> DigestHashAlgorithms = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
            { "SHA-1", HashAlgorithmName.SHA1.Name },
            { "SHA-256", HashAlgorithmName.SHA256.Name },
            { "SHA-384", HashAlgorithmName.SHA384.Name },
            { "SHA-512", HashAlgorithmName.SHA512.Name },
            { "SHA-MD5", HashAlgorithmName.MD5.Name }
        };
    }
}