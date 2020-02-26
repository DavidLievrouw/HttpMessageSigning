using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    internal class CreatedHeaderGuardVerificationTask : IVerificationTask {
        private static readonly string[] AlgorithmNamesThatDoNotAllowCreatedHeader = {"rsa", "hmac", "ecdsa"};

        public Task Verify(HttpRequestForSigning signedRequest, Signature signature, Client client) {
            if (
                signature.Headers.Contains(HeaderName.PredefinedHeaderNames.Created) &&
                AlgorithmNamesThatDoNotAllowCreatedHeader.Contains(client.SignatureAlgorithm.Name, StringComparer.OrdinalIgnoreCase)) {
                throw new SignatureVerificationException(
                    $"It is not allowed to take the {HeaderName.PredefinedHeaderNames.Created} into account, when the signature algorithm is {client.SignatureAlgorithm.Name}.");
            }

            if (!signature.Created.HasValue) {
                throw new SignatureVerificationException($"The signature does not contain a value for the {nameof(signature.Created)} property, but it is required.");
            }

            var createdHeaderValues = signedRequest.Headers.GetValues("Created");
            if (createdHeaderValues != StringValues.Empty && signature.Headers.Contains(HeaderName.PredefinedHeaderNames.Created)) {
                if (!long.TryParse(createdHeaderValues.First(), out var parsedCreatedValue)) {
                    throw new SignatureVerificationException(
                        $"The request does not contain a valid value for the {HeaderName.PredefinedHeaderNames.Created} header.");
                }

                var createdHeaderValue = DateTimeOffset.FromUnixTimeSeconds(parsedCreatedValue);
                if (createdHeaderValue != signature.Created.Value) {
                    throw new SignatureVerificationException(
                        $"The signature creation time does not match the value of the {HeaderName.PredefinedHeaderNames.Created} request header.");
                }
            }

            return Task.CompletedTask;
        }
    }
}