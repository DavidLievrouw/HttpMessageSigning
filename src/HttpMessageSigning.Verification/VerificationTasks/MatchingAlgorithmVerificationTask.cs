using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    internal class MatchingAlgorithmVerificationTask : IVerificationTask {
        private readonly ILogger<MatchingAlgorithmVerificationTask> _logger;
        
        public MatchingAlgorithmVerificationTask(ILogger<MatchingAlgorithmVerificationTask> logger = null) {
            _logger = logger;
        }

        public Task<SignatureVerificationFailure> Verify(HttpRequestForSigning signedRequest, Signature signature, Client client) {
            // Algorithm parameter is not required
            if (string.IsNullOrEmpty(signature.Algorithm)) {
                _logger?.LogDebug("Algorithm match verification is not required, because there is no algorithm specified in the signature.");
                return Task.FromResult<SignatureVerificationFailure>(null);
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
                return SignatureVerificationFailure.InvalidSignatureAlgorithm($"The specified signature algorithm ({signature.Algorithm}) is not supported.")
                    .ToTask<SignatureVerificationFailure>();
            }

            if (!client.SignatureAlgorithm.Name.Equals(algorithmParts[0], StringComparison.InvariantCultureIgnoreCase)) {
                return SignatureVerificationFailure.InvalidSignatureAlgorithm(
                    $"The specified signature algorithm ({algorithmParts[0]}) does not match the registered signature algorithm for the client with id {client.Id}.")
                    .ToTask<SignatureVerificationFailure>();
            }

            if (!client.SignatureAlgorithm.HashAlgorithm.Name.Equals(algorithmParts[1], StringComparison.InvariantCultureIgnoreCase)) {
                return SignatureVerificationFailure.InvalidSignatureAlgorithm(
                    $"The specified hash algorithm ({algorithmParts[1]}) does not match the registered hash algorithm for the client with id {client.Id}.")
                    .ToTask<SignatureVerificationFailure>();
            }

            return Task.FromResult<SignatureVerificationFailure>(null);
        }
    }
}