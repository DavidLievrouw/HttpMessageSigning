using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Dalion.HttpMessageSigning.Verification.AspNetCore {
    internal class DefaultSignatureParser : ISignatureParser {
        private readonly IAuthenticationHeaderExtractor _authenticationHeaderExtractor;
        private readonly ILogger<DefaultSignatureParser> _logger;

        private const string AuthorizationHeaderName = "Authorization";

        public DefaultSignatureParser(IAuthenticationHeaderExtractor authenticationHeaderExtractor, ILogger<DefaultSignatureParser> logger = null) {
            _authenticationHeaderExtractor = authenticationHeaderExtractor ?? throw new ArgumentNullException(nameof(authenticationHeaderExtractor));
            _logger = logger;
        }

        public SignatureParsingResult Parse(HttpRequest request, SignedRequestAuthenticationOptions options) {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (options == null) throw new ArgumentNullException(nameof(options));

            var authHeader = _authenticationHeaderExtractor.Extract(request);
            if (authHeader == null) {
                return new SignatureParsingFailure($"The specified request does not specify a value for the {AuthorizationHeaderName} header.");
            }

            if (string.IsNullOrEmpty(authHeader.Parameter)) {
                return new SignatureParsingFailure(
                    $"The specified request does not specify a valid authentication parameter in the {AuthorizationHeaderName} header.");
            }
            
            if (authHeader.Scheme != options.Scheme && authHeader.Scheme != options.Scheme) {
                return new SignatureParsingFailure(
                    $"The specified request does not specify the expected {options.Scheme} scheme in the {AuthorizationHeaderName} header.");
            }

            var authParam = authHeader.Parameter;
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
                const string keyIdSelector = "keyId=";
                if (authParamPart.StartsWith(keyIdSelector, StringComparison.Ordinal)) {
                    if (keyId != KeyId.Empty) return new SignatureParsingFailure($"Duplicate '{keyIdSelector}' found in signature.");
                    var value = authParamPart[keyIdSelector.Length..].Trim('"');
                    keyId = new KeyId(value);
                }

                const string  algorithmSelector = "algorithm=";
                if (authParamPart.StartsWith(algorithmSelector, StringComparison.Ordinal)) {
                    if (algorithm != string.Empty) return new SignatureParsingFailure($"Duplicate '{algorithmSelector}' found in signature.");
                    var value = authParamPart[algorithmSelector.Length..].Trim('"');
                    algorithm = value;
                }

                const string  createdSelector = "created=";
                if (authParamPart.StartsWith(createdSelector, StringComparison.Ordinal)) {
                    if (createdString != string.Empty) return new SignatureParsingFailure($"Duplicate '{createdSelector}' found in signature.");
                    var value = authParamPart[createdSelector.Length..].Trim('"');
                    createdString = value;
                }

                const string  expiresSelector = "expires=";
                if (authParamPart.StartsWith(expiresSelector, StringComparison.Ordinal)) {
                    if (expiresString != string.Empty) return new SignatureParsingFailure($"Duplicate '{expiresSelector}' found in signature.");
                    var value = authParamPart[expiresSelector.Length..].Trim('"');
                    expiresString = value;
                }

                const string  headersSelector = "headers=";
                if (authParamPart.StartsWith(headersSelector, StringComparison.Ordinal)) {
                    if (headersString != string.Empty) return new SignatureParsingFailure($"Duplicate '{headersSelector}' found in signature.");
                    var value = authParamPart[headersSelector.Length..].Trim('"');
                    headersString = value;
                }

                const string  nonceSelector = "nonce=";
                if (authParamPart.StartsWith(nonceSelector, StringComparison.Ordinal)) {
                    if (!string.IsNullOrEmpty(nonce)) return new SignatureParsingFailure($"Duplicate '{nonceSelector}' found in signature.");
                    var value = authParamPart[nonceSelector.Length..].Trim('"');
                    nonce = value;
                }

                const string  signatureSelector = "signature=";
                if (authParamPart.StartsWith(signatureSelector, StringComparison.Ordinal)) {
                    if (signature != string.Empty) return new SignatureParsingFailure($"Duplicate '{signatureSelector}' found in signature.");
                    var value = authParamPart[signatureSelector.Length..].Trim('"');
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
                return new SignatureParsingFailure(
                    $"The specified request does not specify a valid signature in the {AuthorizationHeaderName} header. See inner exception.",
                    ex);
            }

            return new SignatureParsingSuccess(parsedSignature);
        }
    }
}