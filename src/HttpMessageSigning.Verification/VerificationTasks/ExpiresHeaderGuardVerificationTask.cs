using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    internal class ExpiresHeaderGuardVerificationTask : IVerificationTask {
        private static readonly string[] AlgorithmNamesThatDoNotAllowExpiresHeader = {"rsa", "hmac", "ecdsa"};

        public Task<SignatureVerificationFailure> Verify(HttpRequestForSigning signedRequest, Signature signature, Client client) {
            if (
                signature.Headers.Contains(HeaderName.PredefinedHeaderNames.Expires) &&
                AlgorithmNamesThatDoNotAllowExpiresHeader.Contains(client.SignatureAlgorithm.Name, StringComparer.OrdinalIgnoreCase)) {
                return SignatureVerificationFailure.InvalidExpiresHeader(
                    $"It is not allowed to take the {HeaderName.PredefinedHeaderNames.Expires} into account, when the signature algorithm is {client.SignatureAlgorithm.Name}.")
                    .ToTask<SignatureVerificationFailure>();
            }

            if (!signature.Expires.HasValue) {
                return SignatureVerificationFailure.InvalidExpiresHeader($"The signature does not contain a value for the {nameof(signature.Expires)} property, but it is required.")
                    .ToTask<SignatureVerificationFailure>();
            }

            var expiresHeaderValues = signedRequest.Headers.GetValues(HeaderName.PredefinedHeaderNames.Expires);
            if (expiresHeaderValues != StringValues.Empty && signature.Headers.Contains(HeaderName.PredefinedHeaderNames.Expires)) {
                if (!long.TryParse(expiresHeaderValues.First(), out var parsedExpiresValue)) {
                    return SignatureVerificationFailure.InvalidExpiresHeader(
                        $"The request does not contain a valid value for the {HeaderName.PredefinedHeaderNames.Expires} header.")
                        .ToTask<SignatureVerificationFailure>();
                }

                var expiresHeaderValue = DateTimeOffset.FromUnixTimeSeconds(parsedExpiresValue);
                if (expiresHeaderValue != signature.Expires.Value) {
                    return SignatureVerificationFailure.InvalidExpiresHeader(
                        $"The signature expiration time does not match the value of the {HeaderName.PredefinedHeaderNames.Expires} request header.")
                        .ToTask<SignatureVerificationFailure>();
                }
            }

            return Task.FromResult<SignatureVerificationFailure>(null);
        }
    }
}