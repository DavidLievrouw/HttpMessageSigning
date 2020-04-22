using System;
using System.Linq;
using Microsoft.Extensions.Primitives;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    internal class CreatedHeaderGuardVerificationTask : VerificationTask {
        private static readonly string[] AlgorithmNamesThatLookForDateHeaderHeader = {"rsa", "hmac", "ecdsa"};

        public override SignatureVerificationFailure VerifySync(HttpRequestForSigning signedRequest, Signature signature, Client client) {
            if (!AlgorithmNamesThatLookForDateHeaderHeader.Contains(client.SignatureAlgorithm.Name, StringComparer.OrdinalIgnoreCase) && !signature.Created.HasValue) {
                return SignatureVerificationFailure.InvalidCreatedHeader($"The signature does not contain a value for the {nameof(signature.Created)} property, but it is required.");
            }

            var createdHeaderValues = signedRequest.Headers.GetValues(HeaderName.PredefinedHeaderNames.Created.ToSanitizedHttpHeaderName());
            
            if (createdHeaderValues == StringValues.Empty) {
                // Fallback to '(created)' header
                createdHeaderValues = signedRequest.Headers.GetValues(HeaderName.PredefinedHeaderNames.Created);
            }
            
            if (createdHeaderValues != StringValues.Empty && signature.Headers.Contains(HeaderName.PredefinedHeaderNames.Created)) {
                if (!long.TryParse(createdHeaderValues.First(), out var parsedCreatedValue)) {
                    return SignatureVerificationFailure.InvalidCreatedHeader(
                        $"The request does not contain a valid value for the {HeaderName.PredefinedHeaderNames.Created} header.");
                }

                var createdHeaderValue = DateTimeOffset.FromUnixTimeSeconds(parsedCreatedValue);
                if (createdHeaderValue != signature.Created.Value) {
                    return SignatureVerificationFailure.InvalidCreatedHeader(
                        $"The signature creation time does not match the value of the {HeaderName.PredefinedHeaderNames.Created} request header.");
                }
            }

            return null;
        }
    }
}