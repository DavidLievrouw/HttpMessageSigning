using System;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.Logging;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    internal class MatchingAlgorithmVerificationTask : IVerificationTask {
        private readonly IHttpMessageSigningLogger<MatchingAlgorithmVerificationTask> _logger;
        
        public MatchingAlgorithmVerificationTask(IHttpMessageSigningLogger<MatchingAlgorithmVerificationTask> logger) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<Exception> Verify(HttpRequestForSigning signedRequest, Signature signature, Client client) {
            // Algorithm parameter is not required
            if (string.IsNullOrEmpty(signature.Algorithm)) {
                _logger.Debug("Algorithm match verification is not required, because there is no algorithm specified in the signature.");
                return Task.FromResult<Exception>(null);
            }

            var parts = signature.Algorithm.Split('-');
            if (parts.Length < 2) {
                return new SignatureVerificationException($"The specified signature algorithm ({signature.Algorithm}) is not supported.")
                    .ToTask<Exception>();
            }

            if (!client.SignatureAlgorithm.Name.Equals(parts[0], StringComparison.InvariantCultureIgnoreCase)) {
                return new SignatureVerificationException(
                    $"The specified signature algorithm ({parts[0]}) does not match the registered signature algorithm for the client with id {client.Id}.")
                    .ToTask<Exception>();
            }

            if (!client.SignatureAlgorithm.HashAlgorithm.Name.Equals(parts[1], StringComparison.InvariantCultureIgnoreCase)) {
                return new SignatureVerificationException(
                    $"The specified hash algorithm ({parts[1]}) does not match the registered hash algorithm for the client with id {client.Id}.")
                    .ToTask<Exception>();
            }

            return Task.FromResult<Exception>(null);
        }
    }
}