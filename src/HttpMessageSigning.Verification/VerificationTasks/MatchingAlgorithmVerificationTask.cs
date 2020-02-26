using System;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    internal class MatchingAlgorithmVerificationTask : IVerificationTask {
        public Task Verify(HttpRequestForSigning signedRequest, Signature signature, Client client) {
            // Algorithm parameter is not required
            if (string.IsNullOrEmpty(signature.Algorithm)) return Task.CompletedTask;

            var parts = signature.Algorithm.Split('-');
            if (parts.Length < 2) {
                throw new SignatureVerificationException($"The specified signature algorithm ({signature.Algorithm}) is not supported.");
            }

            if (!client.SignatureAlgorithm.Name.Equals(parts[0], StringComparison.InvariantCultureIgnoreCase)) {
                throw new SignatureVerificationException(
                    $"The specified signature algorithm ({parts[0]}) does not match the registered signature algorithm for the client with id {client.Id}.");
            }

            if (!client.SignatureAlgorithm.HashAlgorithm.Name.Equals(parts[1], StringComparison.InvariantCultureIgnoreCase)) {
                throw new SignatureVerificationException(
                    $"The specified hash algorithm ({parts[1]}) does not match the registered hash algorithm for the client with id {client.Id}.");
            }

            return Task.CompletedTask;
        }
    }
}