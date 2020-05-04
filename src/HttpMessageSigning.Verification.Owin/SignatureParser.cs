using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Owin;

namespace Dalion.HttpMessageSigning.Verification.Owin {
    internal class SignatureParser : ISignatureParser {
        private const string AuthorizationHeaderName = "Authorization";
        private readonly ILogger<SignatureParser> _logger;

        public SignatureParser(ILogger<SignatureParser> logger = null) {
            _logger = logger;
        }

        public Signature Parse(IOwinRequest request, SignedHttpRequestAuthenticationOptions options) {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (options == null) throw new ArgumentNullException(nameof(options));

            var authHeader = request.Headers[AuthorizationHeaderName];
            if (string.IsNullOrEmpty(authHeader)) {
                throw new InvalidSignatureException($"The specified request does not specify a value for the {AuthorizationHeaderName} header.");
            }

            var separatorIndex = authHeader.IndexOf(' ');
            if (separatorIndex < 0) {
                throw new InvalidSignatureException(
                    $"The specified request does not specify a valid authentication parameter in the {AuthorizationHeaderName} header.");
            }

            var authScheme = authHeader.Substring(0, separatorIndex);
            if (authScheme != options.Scheme && authScheme != options.Scheme) {
                throw new InvalidSignatureException(
                    $"The specified request does not specify the expected {options.Scheme} scheme in the {AuthorizationHeaderName} header.");
            }

            if (separatorIndex >= authHeader.Length - 1) {
                throw new InvalidSignatureException(
                    $"The specified request does not specify a valid authentication parameter in the {AuthorizationHeaderName} header.");
            }

            var authParam = authHeader.Substring(separatorIndex + 1);

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
                var keyIdSelector = "keyId=";
                if (authParamPart.StartsWith(keyIdSelector, StringComparison.Ordinal)) {
                    if (keyId != KeyId.Empty) throw new InvalidSignatureException($"Duplicate '{keyIdSelector}' found in signature.");
                    var value = authParamPart.Substring(keyIdSelector.Length).Trim('"');
                    keyId = new KeyId(value);
                }

                var algorithmSelector = "algorithm=";
                if (authParamPart.StartsWith(algorithmSelector, StringComparison.Ordinal)) {
                    if (algorithm != string.Empty) throw new InvalidSignatureException($"Duplicate '{algorithmSelector}' found in signature.");
                    var value = authParamPart.Substring(algorithmSelector.Length).Trim('"');
                    algorithm = value;
                }

                var createdSelector = "created=";
                if (authParamPart.StartsWith(createdSelector, StringComparison.Ordinal)) {
                    if (createdString != string.Empty) throw new InvalidSignatureException($"Duplicate '{createdSelector}' found in signature.");
                    var value = authParamPart.Substring(createdSelector.Length).Trim('"');
                    createdString = value;
                }

                var expiresSelector = "expires=";
                if (authParamPart.StartsWith(expiresSelector, StringComparison.Ordinal)) {
                    if (expiresString != string.Empty) throw new InvalidSignatureException($"Duplicate '{expiresSelector}' found in signature.");
                    var value = authParamPart.Substring(expiresSelector.Length).Trim('"');
                    expiresString = value;
                }

                var headersSelector = "headers=";
                if (authParamPart.StartsWith(headersSelector, StringComparison.Ordinal)) {
                    if (headersString != string.Empty) throw new InvalidSignatureException($"Duplicate '{headersSelector}' found in signature.");
                    var value = authParamPart.Substring(headersSelector.Length).Trim('"');
                    headersString = value;
                }

                var nonceSelector = "nonce=";
                if (authParamPart.StartsWith(nonceSelector, StringComparison.Ordinal)) {
                    if (!string.IsNullOrEmpty(nonce)) throw new InvalidSignatureException($"Duplicate '{nonceSelector}' found in signature.");
                    var value = authParamPart.Substring(nonceSelector.Length).Trim('"');
                    nonce = value;
                }

                var signatureSelector = "signature=";
                if (authParamPart.StartsWith(signatureSelector, StringComparison.Ordinal)) {
                    if (signature != string.Empty) throw new InvalidSignatureException($"Duplicate '{signatureSelector}' found in signature.");
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