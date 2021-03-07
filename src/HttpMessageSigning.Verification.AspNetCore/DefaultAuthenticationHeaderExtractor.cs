using System;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Dalion.HttpMessageSigning.Verification.AspNetCore {
    internal class DefaultAuthenticationHeaderExtractor : IAuthenticationHeaderExtractor {
        private const string AuthorizationHeaderName = "Authorization";

        public AuthenticationHeaderValue Extract(HttpRequest request) {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var authHeader = request.Headers[AuthorizationHeaderName];
            if (authHeader == StringValues.Empty) {
                return null;
            }

            var rawAuthHeader = (string) authHeader;
            var separatorIndex = rawAuthHeader.IndexOf(' ');
            if (separatorIndex < 0) {
                return new AuthenticationHeaderValue(rawAuthHeader);
            }

            var authScheme = rawAuthHeader.Substring(0, separatorIndex);
          
            if (separatorIndex >= rawAuthHeader.Length - 1) {
                return new AuthenticationHeaderValue(authScheme);
            }

            var authParam = rawAuthHeader.Substring(separatorIndex + 1);

            return new AuthenticationHeaderValue(authScheme, authParam);
        }
    }
}