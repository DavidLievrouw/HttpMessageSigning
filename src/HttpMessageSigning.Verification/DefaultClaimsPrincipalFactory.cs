using System;
using System.Linq;
using System.Security.Claims;

namespace Dalion.HttpMessageSigning.Verification {
    internal class DefaultClaimsPrincipalFactory : IClaimsPrincipalFactory {
        private readonly string _version;

        public DefaultClaimsPrincipalFactory() {
            _version = typeof(ISignatureVerifier).Assembly.GetName().Version.ToString(2);
        }

        public ClaimsPrincipal CreateForClient(Client client) {
            if (client == null) throw new ArgumentNullException(nameof(client));
            
            var additionalClaims = client.Claims?.Select(c => new Claim(c.Type, c.Value)) ?? Enumerable.Empty<Claim>();

            return new ClaimsPrincipal(
                new ClaimsIdentity(
                    new[] {
                        new Claim(SignedHttpRequestClaimTypes.AppId, client.Id),
                        new Claim(SignedHttpRequestClaimTypes.Name, client.Name ?? client.Id),
                        new Claim(SignedHttpRequestClaimTypes.Version, _version)
                    }.Concat(additionalClaims),
                    SignedHttpRequestDefaults.AuthenticationScheme,
                    SignedHttpRequestClaimTypes.Name,
                    SignedHttpRequestClaimTypes.Role));
        }
    }
}