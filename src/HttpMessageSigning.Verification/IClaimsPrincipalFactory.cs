using System.Security.Claims;

namespace Dalion.HttpMessageSigning.Verification {
    public interface IClaimsPrincipalFactory {
        ClaimsPrincipal CreateForClient(Client client);
    }
}
