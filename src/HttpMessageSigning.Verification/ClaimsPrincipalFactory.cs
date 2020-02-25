using System;
using System.Linq;
using System.Security.Claims;

namespace Dalion.HttpMessageSigning.Verification {
    internal class ClaimsPrincipalFactory : IClaimsPrincipalFactory {
        public ClaimsPrincipal CreateForClient(Client client) {
            if (client == null) throw new ArgumentNullException(nameof(client));
            
            var additionalClaims = client.Claims?.Select(c => new System.Security.Claims.Claim(c.Type, c.Value)) ?? Enumerable.Empty<System.Security.Claims.Claim>();

            return new ClaimsPrincipal(
                new ClaimsIdentity(
                    additionalClaims.Concat(
                        new[] {
                            new System.Security.Claims.Claim(Constants.ClaimTypes.AppId, client.Id)
                        }),
                    Constants.AuthenticationSchemes.HttpRequestSignature,
                    Constants.ClaimTypes.AppId,
                    Constants.ClaimTypes.Role));
        }
    }
}