using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    internal class MatchingAlgorithmVerificationTask : VerificationTask {
        private readonly ILogger<MatchingAlgorithmVerificationTask> _logger;
        
        public MatchingAlgorithmVerificationTask(ILogger<MatchingAlgorithmVerificationTask> logger = null) {
            _logger = logger;
        }

        public override SignatureVerificationFailure VerifySync(HttpRequestForSigning signedRequest, Signature signature, Client client) {
            // Algorithm parameter is not required
            if (string.IsNullOrEmpty(signature.Algorithm)) {
                _logger?.LogDebug("Algorithm match verification is not required, because there is no algorithm specified in the signature.");
                return null;
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
                return SignatureVerificationFailure.InvalidSignatureAlgorithm($"The specified signature algorithm ({signature.Algorithm}) is not supported.");
            }

            if (!client.SignatureAlgorithm.Name.Equals(algorithmParts[0], StringComparison.InvariantCultureIgnoreCase)) {
                return SignatureVerificationFailure.InvalidSignatureAlgorithm($"The specified signature algorithm ({algorithmParts[0]}) does not match the registered signature algorithm for the client with id {client.Id}.");
            }

            if (!client.SignatureAlgorithm.HashAlgorithm.Name.Equals(algorithmParts[1], StringComparison.InvariantCultureIgnoreCase)) {
                return SignatureVerificationFailure.InvalidSignatureAlgorithm($"The specified hash algorithm ({algorithmParts[1]}) does not match the registered hash algorithm for the client with id {client.Id}.");
            }

            return null;
        }
    }
}