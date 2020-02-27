using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    internal class KnownAlgorithmVerificationTask : IVerificationTask {
        private readonly ILogger<KnownAlgorithmVerificationTask> _logger;
        
        private static readonly string[] SupportedSignatureAlgorithmNames = {"rsa", "hmac"};

        public KnownAlgorithmVerificationTask(ILogger<KnownAlgorithmVerificationTask> logger) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<Exception> Verify(HttpRequestForSigning signedRequest, Signature signature, Client client) {
            // Algorithm parameter is not required
            if (string.IsNullOrEmpty(signature.Algorithm)) {
                _logger.LogDebug("Algorithm verification is not required, because there is no algorithm specified in the signature.");
                return Task.FromResult<Exception>(null);
            }

            var algorithmParts = new List<string>();
            if (!string.IsNullOrEmpty(signature.Algorithm)) {
                var separatorIndex = signature.Algorithm.IndexOf('-');
                if (separatorIndex < 0 || separatorIndex >= signature.Algorithm.Length - 1) {
                    algorithmParts.Add(signature.Algorithm);
                }
                else {
                    algorithmParts.Add(signature.Algorithm.Substring(0, separatorIndex));
                    algorithmParts.Add(signature.Algorithm.Substring(separatorIndex + 1));
                }
            }
            
            if (algorithmParts.Count < 2) {
                return new SignatureVerificationException($"The specified signature algorithm ({signature.Algorithm}) is not supported.")
                    .ToTask<Exception>();
            }

            if (!SupportedSignatureAlgorithmNames.Contains(algorithmParts[0])) {
                return new SignatureVerificationException($"The specified signature algorithm ({signature.Algorithm}) is not supported.")
                    .ToTask<Exception>();
            }

            var hashAlgorithm = System.Security.Cryptography.HashAlgorithm.Create(algorithmParts[1].ToUpperInvariant());
            if (hashAlgorithm == null) {
                return new SignatureVerificationException($"The specified hash algorithm ({algorithmParts[1]}) is not supported.")
                    .ToTask<Exception>();
            }
            
            return Task.FromResult<Exception>(null);
        }
    }
}