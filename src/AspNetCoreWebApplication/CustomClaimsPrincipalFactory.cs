using System;
using System.Linq;
using System.Security.Claims;
using Dalion.HttpMessageSigning.Verification;
using Dalion.HttpMessageSigning.Verification.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace WebApplication {
    public class CustomClaimsPrincipalFactory : IClaimsPrincipalFactory {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CustomClaimsPrincipalFactory(IHttpContextAccessor httpContextAccessor) {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public ClaimsPrincipal CreateForClient(Client client) {
            if (client == null) throw new ArgumentNullException(nameof(client));

            var additionalClaims = client.Claims?.Select(c => new Claim(c.Type, c.Value)) ?? Enumerable.Empty<Claim>();

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.Request != null) {
                additionalClaims = additionalClaims.Concat(new[] {new Claim("path", httpContext.Request.GetDisplayUrl())});
            }
            
            return new ClaimsPrincipal(
                new ClaimsIdentity(
                    new[] {
                        new Claim(SignedHttpRequestClaimTypes.AppId, client.Id),
                        new Claim("application", "ASP.NET Core Web Application"),
                        new Claim(SignedHttpRequestClaimTypes.Name, client.Name ?? client.Id),
                        new Claim(SignedHttpRequestClaimTypes.Version, typeof(IRequestSignatureVerifier).Assembly.GetName().Version!.ToString(2))
                    }.Concat(additionalClaims),
                    SignedHttpRequestDefaults.AuthenticationScheme,
                    SignedHttpRequestClaimTypes.Name,
                    SignedHttpRequestClaimTypes.Role));
        }
    }
}