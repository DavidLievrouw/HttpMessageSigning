using System;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace Dalion.HttpMessageSigning.Verification {
    internal class SignatureParser : ISignatureParser {
        private const string AuthorizationHeaderName = "Authorization";
        private const string AuthorizationScheme = "Signature";

        private static readonly Regex KeyIdRegEx = new Regex("keyId=\"(?<keyId>[A-z0-9, =-]+)\"", RegexOptions.Compiled);
        private static readonly Regex AlgorithmRegEx = new Regex("algorithm=\"(?<algorithm>[a-z0-9-]+)\"", RegexOptions.Compiled);
        private static readonly Regex CreatedRegEx = new Regex("created=(?<created>[0-9]+)", RegexOptions.Compiled);
        private static readonly Regex ExpiresRegEx = new Regex("expires=(?<expires>[0-9]+)", RegexOptions.Compiled);
        private static readonly Regex HeadersRegEx = new Regex("headers=\"(?<headers>[a-z0-9-\\(\\) ]+)\"", RegexOptions.Compiled);
        private static readonly Regex SignatureRegEx = new Regex("signature=\"(?<signature>[a-zA-Z0-9+/]+={0,2})\"", RegexOptions.Compiled);

        public Signature Parse(HttpRequestMessage request) {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var authHeader = request.Headers.Authorization;
            if (authHeader == null) {
                throw new SignatureVerificationException($"The specified request does not specify a value for the {AuthorizationHeaderName} header.");
            }
            if (string.IsNullOrEmpty(authHeader.Parameter)) {
                throw new SignatureVerificationException($"The specified request does not specify a parameter for the {AuthorizationHeaderName} header.");
            }

            if (authHeader.Scheme != AuthorizationScheme) {
                throw new SignatureVerificationException(
                    $"The specified request does not specify the {AuthorizationScheme} scheme in the {AuthorizationHeaderName} header.");
            }

            var keyId = KeyId.Empty;
            var keyIdMatch = KeyIdRegEx.Match(authHeader.Parameter);
            if (keyIdMatch.Success) keyId = (KeyId) keyIdMatch.Groups["keyId"].Value;

            var algorithm = string.Empty;
            var algMatch = AlgorithmRegEx.Match(authHeader.Parameter);
            if (algMatch.Success) algorithm = algMatch.Groups["algorithm"].Value;

            var createdString = string.Empty;
            var createdMatch = CreatedRegEx.Match(authHeader.Parameter);
            if (createdMatch.Success) createdString = createdMatch.Groups["created"].Value;

            var expiresString = string.Empty;
            var expiresMatch = ExpiresRegEx.Match(authHeader.Parameter);
            if (expiresMatch.Success) expiresString = expiresMatch.Groups["expires"].Value;

            var headersString = string.Empty;
            var headersMatch = HeadersRegEx.Match(authHeader.Parameter);
            if (headersMatch.Success) headersString = headersMatch.Groups["headers"].Value;

            var signature = string.Empty;
            var signatureMatch = SignatureRegEx.Match(authHeader.Parameter);
            if (signatureMatch.Success) signature = signatureMatch.Groups["signature"].Value;

            DateTimeOffset? created = null;
            if (long.TryParse(createdString, out var createdEpoch)) {
                created = DateTimeOffset.FromUnixTimeSeconds(createdEpoch);
            }

            DateTimeOffset? expires = null;
            if (long.TryParse(expiresString, out var expiresEpoch)) {
                expires = DateTimeOffset.FromUnixTimeSeconds(expiresEpoch);
            }

            var headerNames = Array.Empty<HeaderName>();
            if (!string.IsNullOrEmpty(headersString)) {
                headerNames = headersString
                    .Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => new HeaderName(s)).ToArray();
            }

            var parsedSignature = new Signature {
                KeyId = keyId,
                Algorithm = string.IsNullOrEmpty(algorithm) ? null : algorithm.Trim(),
                Created = created,
                Expires = expires,
                Headers = headerNames.Any() ? headerNames : null,
                String = signature
            };

            try {
                parsedSignature.Validate();
            }
            catch (ValidationException ex) {
                throw new SignatureVerificationException(
                    $"The specified request does not specify a valid signature in the {AuthorizationHeaderName} header. See inner exception.",
                    ex);
            }

            return parsedSignature;
        }
    }
}