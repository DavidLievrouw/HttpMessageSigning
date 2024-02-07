using System;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Dalion.HttpMessageSigning.Verification.AspNetCore {
    internal class DefaultAuthenticationHeaderExtractor : IAuthenticationHeaderExtractor {
        private const string AuthorizationHeaderName = "Authorization";
        private readonly ILogger<DefaultAuthenticationHeaderExtractor> _logger;

        public DefaultAuthenticationHeaderExtractor(ILogger<DefaultAuthenticationHeaderExtractor> logger = null) {
            _logger = logger;
        }

        public AuthenticationHeaderValue Extract(HttpRequest request) {
            if (request == null) throw new ArgumentNullException(nameof(request));

            try {
                var authHeader = request.Headers[AuthorizationHeaderName];
                if (authHeader == StringValues.Empty) {
                    return null;
                }

                var rawAuthHeader = (string) authHeader;
                var separatorIndex = rawAuthHeader.IndexOf(' ');
                if (separatorIndex < 0) {
                    return new AuthenticationHeaderValue(rawAuthHeader);
                }

                var authScheme = rawAuthHeader[..separatorIndex];
          
                if (separatorIndex >= rawAuthHeader.Length - 1) {
                    return new AuthenticationHeaderValue(authScheme);
                }

                var authParam = rawAuthHeader[(separatorIndex + 1)..];

                return new AuthenticationHeaderValue(authScheme, authParam);
            }
            catch (FormatException ex) {
                _logger?.LogWarning(ex, $"The {nameof(AuthenticationHeaderValue)} could not be extracted from the request. See exception for details.");
                return null;
            }
        }
    }
}