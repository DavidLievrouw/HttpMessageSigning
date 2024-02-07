using System;
using System.Net.Http.Headers;
using Dalion.HttpMessageSigning.Verification.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace WebApplication {
    public class CustomHeaderSupportingAuthenticationHeaderExtractor : IAuthenticationHeaderExtractor {
        private readonly string _headerName;
        private readonly ILogger<CustomHeaderSupportingAuthenticationHeaderExtractor> _logger;

        public CustomHeaderSupportingAuthenticationHeaderExtractor(
            string headerName = "Authorization",
            ILogger<CustomHeaderSupportingAuthenticationHeaderExtractor> logger = null) {
            _headerName = headerName ?? throw new ArgumentNullException(nameof(headerName));
            _logger = logger;
        }

        public AuthenticationHeaderValue Extract(HttpRequest request) {
            if (request == null) throw new ArgumentNullException(nameof(request));

            try {
                var authHeader = request.Headers[_headerName];
                if (authHeader == StringValues.Empty) {
                    return null;
                }

                var rawAuthHeader = (string)authHeader;
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