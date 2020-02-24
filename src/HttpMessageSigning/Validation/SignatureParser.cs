using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Dalion.HttpMessageSigning.Validation {
    public class SignatureParser : ISignatureParser {
        private const string AuthorizationHeaderName = "Authorization";
        private const string AuthorizationScheme = "Signature";

        private static readonly Regex KeyIdRegEx = new Regex("keyId=\"(?<keyId>[A-z0-9, =-]+)\"", RegexOptions.Compiled);
        private static readonly Regex AlgorithmRegEx = new Regex("algorithm=\"(?<algorithm>[a-z0-9-]+)\"", RegexOptions.Compiled);
        private static readonly Regex CreatedRegEx = new Regex("created=(?<created>[0-9]+)", RegexOptions.Compiled);
        private static readonly Regex ExpiresRegEx = new Regex("expires=(?<expires>[0-9]+)", RegexOptions.Compiled);
        private static readonly Regex HeadersRegEx = new Regex("headers=\"(?<headers>[a-z0-9-\\(\\) ]+)\"", RegexOptions.Compiled);
        private static readonly Regex SignatureRegEx = new Regex("signature=\"(?<signature>[a-zA-Z0-9+/]+={0,2})\"", RegexOptions.Compiled);

        public Signature Parse(HttpRequest request) {
            var authHeader = request.Headers[AuthorizationHeaderName];
            if (authHeader == StringValues.Empty)
                throw new HttpMessageSigningInvalidRequestException($"The specified request does not specify a value for the {AuthorizationHeaderName} header.");

            var authHeaderParts = ((string) authHeader).Split(' ');
            var authScheme = ((string) authHeader).Split(' ')[0];
            if (authScheme != AuthorizationScheme)
                throw new HttpMessageSigningInvalidRequestException(
                    $"The specified request does not specify the {AuthorizationScheme} scheme in the {AuthorizationHeaderName} header.");

            if (authHeaderParts.Length < 2)
                throw new HttpMessageSigningInvalidRequestException(
                    $"The specified request does not specify a valid authentication parameter in the {AuthorizationHeaderName} header.");
            var authParam = string.Join("", authHeader.Skip(authScheme.Length + 1));

            var keyIdString = string.Empty;
            var keyIdMatch = KeyIdRegEx.Match(authParam);
            if (keyIdMatch.Success) keyIdString = keyIdMatch.Groups["keyId"].Value;

            var algString = string.Empty;
            var algMatch = AlgorithmRegEx.Match(authParam);
            if (algMatch.Success) algString = algMatch.Groups["algorithm"].Value;

            var createdString = string.Empty;
            var createdMatch = CreatedRegEx.Match(authParam);
            if (createdMatch.Success) createdString = createdMatch.Groups["created"].Value;

            var expiresString = string.Empty;
            var expiresMatch = ExpiresRegEx.Match(authParam);
            if (expiresMatch.Success) expiresString = expiresMatch.Groups["expires"].Value;

            var headersString = string.Empty;
            var headersMatch = HeadersRegEx.Match(authParam);
            if (headersMatch.Success) headersString = headersMatch.Groups["headers"].Value;

            var signature = string.Empty;
            var signatureMatch = SignatureRegEx.Match(authParam);
            if (signatureMatch.Success) signature = signatureMatch.Groups["signature"].Value;

            if (!KeyId.TryParse(keyIdString, out var keyId)) {
                throw new HttpMessageSigningInvalidRequestException(
                    $"The specified request does not specify a valid keyId in the authentication parameter of the {AuthorizationHeaderName} header.");
            }

            /*if (!Enum.TryParse(algString, out Algorithm algorithm)) {
                throw new HttpMessageSigningInvalidRequestException(
                    $"The specified request does not specify a valid algorithm in the authentication parameter of the {AuthorizationHeaderName} header.");
            }*/

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
                headerNames = headersString.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries).Select(s => new HeaderName(s)).ToArray();
            }

            return new Signature {
                KeyId = keyId,
                //Algorithm = algorithm, // ToDo: Parse Algorithm
                Created = created,
                Expires = expires,
                Headers = headerNames,
                String = signature
            };
        }
    }
}