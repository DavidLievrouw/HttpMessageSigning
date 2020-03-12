using System;
using System.Linq;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    internal class ExpiresHeaderGuardVerificationTask : IVerificationTask {
        private static readonly string[] AlgorithmNamesThatDoNotAllowExpiresHeader = {"rsa", "hmac", "ecdsa"};

        public Task<Exception> Verify(HttpRequestForSigning signedRequest, Signature signature, Client client) {
            if (
                signature.Headers.Contains(HeaderName.PredefinedHeaderNames.Expires) &&
                AlgorithmNamesThatDoNotAllowExpiresHeader.Contains(client.SignatureAlgorithm.Name, StringComparer.OrdinalIgnoreCase)) {
                return new SignatureVerificationException(
                    $"It is not allowed to take the {HeaderName.PredefinedHeaderNames.Expires} into account, when the signature algorithm is {client.SignatureAlgorithm.Name}.")
                    .ToTask<Exception>();
            }

            if (!signature.Expires.HasValue) {
                return new SignatureVerificationException($"The signature does not contain a value for the {nameof(signature.Expires)} property, but it is required.")
                    .ToTask<Exception>();
            }

            var expiresHeaderValues = signedRequest.Headers.GetValues(HeaderName.PredefinedHeaderNames.Expires);
            if (expiresHeaderValues != StringValues.Empty && signature.Headers.Contains(HeaderName.PredefinedHeaderNames.Expires)) {
                if (!long.TryParse(expiresHeaderValues.First(), out var parsedExpiresValue)) {
                    return new SignatureVerificationException(
                        $"The request does not contain a valid value for the {HeaderName.PredefinedHeaderNames.Expires} header.")
                        .ToTask<Exception>();
                }

                var expiresHeaderValue = DateTimeOffset.FromUnixTimeSeconds(parsedExpiresValue);
                if (expiresHeaderValue != signature.Expires.Value) {
                    return new SignatureVerificationException(
                        $"The signature expiration time does not match the value of the {HeaderName.PredefinedHeaderNames.Expires} request header.")
                        .ToTask<Exception>();
                }
            }

            return Task.FromResult<Exception>(null);
        }
    }
}