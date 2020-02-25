using System.Security.Claims;

namespace Dalion.HttpMessageSigning.Verification {
    internal interface IClaimsPrincipalFactory {
        ClaimsPrincipal CreateForClient(Client client);
    }
}