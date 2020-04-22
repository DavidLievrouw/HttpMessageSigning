using System;
using System.Linq;
using Microsoft.Extensions.Primitives;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    internal class ExpiresHeaderGuardVerificationTask : VerificationTask {
        private static readonly string[] AlgorithmNamesThatLookForDateHeaderHeader = {"rsa", "hmac", "ecdsa"};

        public override SignatureVerificationFailure VerifySync(HttpRequestForSigning signedRequest, Signature signature, Client client) {
            if (!AlgorithmNamesThatLookForDateHeaderHeader.Contains(client.SignatureAlgorithm.Name, StringComparer.OrdinalIgnoreCase) && !signature.Expires.HasValue) {
                return SignatureVerificationFailure.InvalidExpiresHeader($"The signature does not contain a value for the {nameof(signature.Expires)} property, but it is required.");
            }

            var expiresHeaderValues = signedRequest.Headers.GetValues(HeaderName.PredefinedHeaderNames.Expires);
            if (expiresHeaderValues != StringValues.Empty && signature.Headers.Contains(HeaderName.PredefinedHeaderNames.Expires)) {
                if (!long.TryParse(expiresHeaderValues.First(), out var parsedExpiresValue)) {
                    return SignatureVerificationFailure.InvalidExpiresHeader(
                        $"The request does not contain a valid value for the {HeaderName.PredefinedHeaderNames.Expires} header.");
                }

                var expiresHeaderValue = DateTimeOffset.FromUnixTimeSeconds(parsedExpiresValue);
                if (expiresHeaderValue != signature.Expires.Value) {
                    return SignatureVerificationFailure.InvalidExpiresHeader(
                        $"The signature expiration time does not match the value of the {HeaderName.PredefinedHeaderNames.Expires} request header.");
                }
            }

            return null;
        }
    }
}