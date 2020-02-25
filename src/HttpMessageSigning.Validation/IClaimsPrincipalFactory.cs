using System.Security.Claims;

namespace Dalion.HttpMessageSigning.Validation {
    internal interface IClaimsPrincipalFactory {
        ClaimsPrincipal CreateForClient(Client client);
    }
}