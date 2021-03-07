using System;
using System.Net.Http.Headers;
using Dalion.HttpMessageSigning.Verification.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace WebApplication {
    public class CustomHeaderSupportingAuthenticationHeaderExtractor : IAuthenticationHeaderExtractor {
        private readonly string _headerName;

        public CustomHeaderSupportingAuthenticationHeaderExtractor(string headerName = "Authorization") {
            _headerName = headerName ?? throw new ArgumentNullException(nameof(headerName));
        }

        public AuthenticationHeaderValue Extract(HttpRequest request) {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var authHeader = request.Headers[_headerName];
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