using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    internal class ExpiresHeaderGuardVerificationTask : IVerificationTask {
        private static readonly string[] AlgorithmNamesThatDoNotAllowExpiresHeader = {"rsa", "hmac", "ecdsa"};

        public Task Verify(HttpRequestForSigning signedRequest, Signature signature, Client client) {
            if (
                signature.Headers.Contains(HeaderName.PredefinedHeaderNames.Expires) &&
                AlgorithmNamesThatDoNotAllowExpiresHeader.Contains(client.SignatureAlgorithm.Name, StringComparer.OrdinalIgnoreCase)) {
                throw new SignatureVerificationException(
                    $"It is not allowed to take the {HeaderName.PredefinedHeaderNames.Expires} into account, when the signature algorithm is {client.SignatureAlgorithm.Name}.");
            }

            if (!signature.Expires.HasValue) {
                throw new SignatureVerificationException($"The signature does not contain a value for the {nameof(signature.Expires)} property, but it is required.");
            }

            var expiresHeaderValues = signedRequest.Headers.GetValues("Expires");
            if (expiresHeaderValues != StringValues.Empty && signature.Headers.Contains(HeaderName.PredefinedHeaderNames.Expires)) {
                if (!long.TryParse(expiresHeaderValues.First(), out var parsedExpiresValue)) {
                    throw new SignatureVerificationException(
                        $"The request does not contain a valid value for the {HeaderName.PredefinedHeaderNames.Expires} header.");
                }

                var expiresHeaderValue = DateTimeOffset.FromUnixTimeSeconds(parsedExpiresValue);
                if (expiresHeaderValue != signature.Expires.Value) {
                    throw new SignatureVerificationException(
                        $"The signature creation time does not match the value of the {HeaderName.PredefinedHeaderNames.Expires} request header.");
                }
            }

            return Task.CompletedTask;
        }
    }
}