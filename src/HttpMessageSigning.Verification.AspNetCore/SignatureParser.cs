using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Dalion.HttpMessageSigning.Verification.AspNetCore {
    internal class SignatureParser : ISignatureParser {
        private readonly ILogger<SignatureParser> _logger;
        
        private const string AuthorizationHeaderName = "Authorization";
        private const string LegacyAuthorizationScheme = "SignedHttpRequest";
        private const string AuthorizationScheme = "Signature";

        public SignatureParser(ILogger<SignatureParser> logger = null) {
            _logger = logger;
        }

        public Signature Parse(HttpRequest request) {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var authHeader = request.Headers[AuthorizationHeaderName];
            if (authHeader == Microsoft.Extensions.Primitives.StringValues.Empty)
                throw new InvalidSignatureException($"The specified request does not specify a value for the {AuthorizationHeaderName} header.");

            var rawAuthHeader = (string) authHeader;
            var separatorIndex = rawAuthHeader.IndexOf(' ');
            if (separatorIndex < 0) {
                throw new InvalidSignatureException(
                    $"The specified request does not specify a valid authentication parameter in the {AuthorizationHeaderName} header.");
            }
            var authScheme = rawAuthHeader.Substring(0, separatorIndex);
            if (authScheme != AuthorizationScheme && authScheme != LegacyAuthorizationScheme)
                throw new InvalidSignatureException(
                    $"The specified request does not specify the expected {AuthorizationScheme} scheme in the {AuthorizationHeaderName} header.");

            if (separatorIndex >= rawAuthHeader.Length - 1)
                throw new InvalidSignatureException(
                    $"The specified request does not specify a valid authentication parameter in the {AuthorizationHeaderName} header.");
            var authParam = rawAuthHeader.Substring(separatorIndex + 1);

            _logger?.LogDebug("Parsing authorization header parameter for verification: {0}.", authParam);

            var authParamParts = authParam.Split(',');
            
            var keyId = KeyId.Empty;
            var algorithm = string.Empty;
            var createdString = string.Empty;
            var expiresString = string.Empty;
            var headersString = string.Empty;
            string nonce = null;
            var signature = string.Empty;
            
            foreach (var authParamPart in authParamParts) {
                if (authParamPart == null) continue;
                
                var keyIdSelector = "keyId=";
                if (authParamPart.StartsWith(keyIdSelector, StringComparison.Ordinal)) {
                    var value = authParamPart.Substring(keyIdSelector.Length).Trim('"');
                    keyId = new KeyId(value);
                }
                
                var algorithmSelector = "algorithm=";
                if (authParamPart.StartsWith(algorithmSelector, StringComparison.Ordinal)) {
                    var value = authParamPart.Substring(algorithmSelector.Length).Trim('"');
                    algorithm = value;
                }
                
                var createdSelector = "created=";
                if (authParamPart.StartsWith(createdSelector, StringComparison.Ordinal)) {
                    var value = authParamPart.Substring(createdSelector.Length).Trim('"');
                    createdString = value;
                }
                
                var expiresSelector = "expires=";
                if (authParamPart.StartsWith(expiresSelector, StringComparison.Ordinal)) {
                    var value = authParamPart.Substring(expiresSelector.Length).Trim('"');
                    expiresString = value;
                }
                
                var headersSelector = "headers=";
                if (authParamPart.StartsWith(headersSelector, StringComparison.Ordinal)) {
                    var value = authParamPart.Substring(headersSelector.Length).Trim('"');
                    headersString = value;
                }
                
                var nonceSelector = "nonce=";
                if (authParamPart.StartsWith(nonceSelector, StringComparison.Ordinal)) {
                    var value = authParamPart.Substring(nonceSelector.Length).Trim('"');
                    nonce = value;
                }
                
                var signatureSelector = "signature=";
                if (authParamPart.StartsWith(signatureSelector, StringComparison.Ordinal)) {
                    var value = authParamPart.Substring(signatureSelector.Length).Trim('"');
                    signature = value;
                }
            }
  
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
                Nonce = nonce,
                String = signature
            };
            
            try {
                parsedSignature.Validate();
            }
            catch (ValidationException ex) {
                throw new InvalidSignatureException(
                    $"The specified request does not specify a valid signature in the {AuthorizationHeaderName} header. See inner exception.",
                    ex);
            }

            return parsedSignature;
        }
    }
}