using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    internal class KnownAlgorithmVerificationTask : VerificationTask {
        private readonly ILogger<KnownAlgorithmVerificationTask> _logger;
        
        private static readonly string[] SupportedSignatureAlgorithmNames = {"rsa", "ecdsa", "hmac", "hs2019"};
        private static readonly string[] SupportedHashAlgorithmNames = {
            HashAlgorithmName.MD5.Name,
            HashAlgorithmName.SHA1.Name,
            HashAlgorithmName.SHA256.Name,
            HashAlgorithmName.SHA384.Name,
            HashAlgorithmName.SHA512.Name
        };

        public KnownAlgorithmVerificationTask(ILogger<KnownAlgorithmVerificationTask> logger = null) {
            _logger = logger;
        }

        public override SignatureVerificationFailure VerifySync(HttpRequestForVerification signedRequest, Signature signature, Client client) {
            // Algorithm parameter is not required
            if (string.IsNullOrEmpty(signature.Algorithm)) {
                _logger?.LogDebug("Algorithm verification is not required, because there is no algorithm specified in the signature.");
                return null;
            }

            // hs2019 is always allowed
            if (signature.Algorithm == Signature.DefaultSignatureAlgorithm) return null;
            
            var algorithmParts = new List<string>();
            if (!string.IsNullOrEmpty(signature.Algorithm)) {
                var separatorIndex = signature.Algorithm.IndexOf('-');
                if (separatorIndex < 0 || separatorIndex >= signature.Algorithm.Length - 1) {
                    algorithmParts.Add(signature.Algorithm);
                }
                else {
#if NET6_0_OR_GREATER
                    algorithmParts.Add(signature.Algorithm[..separatorIndex]);
                    algorithmParts.Add(signature.Algorithm[(separatorIndex + 1)..]);
#else
                    algorithmParts.Add(signature.Algorithm.Substring(0, separatorIndex));
                    algorithmParts.Add(signature.Algorithm.Substring(separatorIndex + 1));
#endif
                }
            }
            
            if (algorithmParts.Count < 2) {
                return SignatureVerificationFailure.InvalidSignatureAlgorithm($"The specified signature algorithm ({signature.Algorithm}) is not supported.");
            }

            if (!SupportedSignatureAlgorithmNames.Contains(algorithmParts[0])) {
                return SignatureVerificationFailure.InvalidSignatureAlgorithm($"The specified signature algorithm ({signature.Algorithm}) is not supported.");
            }

            if (!SupportedHashAlgorithmNames.Contains(algorithmParts[1].ToUpperInvariant())) {
                return SignatureVerificationFailure.InvalidSignatureAlgorithm($"The specified hash algorithm ({algorithmParts[1]}) is not supported.");
            }
            
            return null;
        }
    }
}