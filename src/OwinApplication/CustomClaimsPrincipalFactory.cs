using System;
using System.Linq;
using System.Security.Claims;
using System.Web;
using Dalion.HttpMessageSigning.Verification;
using Dalion.HttpMessageSigning.Verification.Owin;

namespace OwinApplication {
    public class CustomClaimsPrincipalFactory : IClaimsPrincipalFactory {
        public ClaimsPrincipal CreateForClient(Client client) {
            if (client == null) throw new ArgumentNullException(nameof(client));

            var additionalClaims = client.Claims?.Select(c => new Claim(c.Type, c.Value)) ?? Enumerable.Empty<Claim>();

            var httpContext = HttpContext.Current;
            if (httpContext?.Request != null) {
                additionalClaims = additionalClaims.Concat(new[] {new Claim("path", httpContext.Request.RawUrl)});
            }
            
            return new ClaimsPrincipal(
                new ClaimsIdentity(
                    new[] {
                        new Claim(SignedHttpRequestClaimTypes.AppId, client.Id),
                        new Claim("application", "Owin Web Application"),
                        new Claim(SignedHttpRequestClaimTypes.Name, client.Name ?? client.Id),
                        new Claim(SignedHttpRequestClaimTypes.Version, typeof(IRequestSignatureVerifier).Assembly.GetName().Version.ToString(2))
                    }.Concat(additionalClaims),
                    SignedHttpRequestDefaults.AuthenticationScheme,
                    SignedHttpRequestClaimTypes.Name,
                    SignedHttpRequestClaimTypes.Role));
        }
    }
}